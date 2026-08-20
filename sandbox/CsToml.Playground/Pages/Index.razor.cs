using BlazorMonaco;
using BlazorMonaco.Editor;
using CsToml.Error;
using CsToml.Playground.Models;
using CsToml.Playground.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace CsToml.Playground.Pages;

public partial class Index : IDisposable
{
    private const string MinimumVersion = "1.0.0";
    private const long MaxFileSize = 1024 * 1024; // 1MB

    [AllowNull]
    private StandaloneCodeEditor editor;

    [AllowNull]
    private StandaloneCodeEditor outputEditor;

    [Inject]
    public IConfiguration? Configuration { get; set; }

    [Inject]
    public IThemeService? ThemeService { get; set; }

    private bool editorReady;
    private bool outputReady;
    private bool isValid = true;
    private bool copied;
    private bool isDarkMode;
    private string outputTab = "json";
    private string mobileView = "code";
    private string selectedSpec = MinimumVersion;
    private string selectedSample = TomlSamples.All[0].Key;
    private string editorTheme = "vs";
    private string? fileUploadError;
    private string? githubUrl;
    private string libraryVersion = "";
    private string jsonOutput = "";
    private string tomlOutput = "";
    private double lastParseMilliseconds;
    private CancellationTokenSource? debounceCts;
    private readonly List<(long Line, string Message)> problems = [];

    private bool allowUnicodeInBareKeys;
    private bool allowNewlinesInInlineTables;
    private bool allowTrailingCommaInInlineTables;
    private bool allowSecondsOmissionInTime;
    private bool supportsEscapeSequenceE;
    private bool supportsEscapeSequenceX;

    protected override void OnInitialized()
    {
        githubUrl = Configuration?["githubUrl"];
        selectedSpec = Configuration?["defaultTomlVersion"] ?? MinimumVersion;
        ApplySpecPreset();

        var assembly = typeof(CsTomlSerializer).Assembly;
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString(3)
            ?? "";
        var plusIndex = version.IndexOf('+');
        libraryVersion = $"v{(plusIndex >= 0 ? version[..plusIndex] : version)}";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        // Restore the persisted app theme (ThemeService reads its stored settings / system preference).
        var dark = ThemeService is not null && await ThemeService.IsDarkModeAsync();
        if (dark != isDarkMode)
        {
            isDarkMode = dark;
            await SyncEditorThemeToAppThemeAsync();
            StateHasChanged();
        }
    }

    private StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
    {
        return new StandaloneEditorConstructionOptions
        {
            Language = "toml",
            AutomaticLayout = true,
            GlyphMargin = true,
            Minimap = new EditorMinimapOptions { Enabled = false },
            ScrollBeyondLastLine = false,
            FontSize = 14,
            TabSize = 2,
            RenderLineHighlight = "gutter",
            Theme = editorTheme,
            Value = TomlSamples.All[0].Content,
        };
    }

    private StandaloneEditorConstructionOptions OutputEditorConstructionOptions(StandaloneCodeEditor editor)
    {
        return new StandaloneEditorConstructionOptions
        {
            Language = "json",
            AutomaticLayout = true,
            ReadOnly = true,
            Minimap = new EditorMinimapOptions { Enabled = false },
            ScrollBeyondLastLine = false,
            FontSize = 14,
            RenderLineHighlight = "none",
            LineNumbers = "off",
            Theme = editorTheme,
            Value = "",
        };
    }

    private async Task OnEditorInitAsync()
    {
        editorReady = true;

        // The TOML language is registered by toml-monarch.js after monaco materializes;
        // re-apply it here in case the model was created before registration completed.
        var model = await editor.GetModel();
        await Global.SetModelLanguage(JS, model, "toml");

        // Editor creation can race with the theme restore in OnAfterRenderAsync, and the
        // construction options set the global monaco theme; re-apply the resolved theme.
        await Global.SetTheme(JS, editorTheme);

        await ValidateAsync();
    }

    private async Task OnOutputEditorInitAsync()
    {
        outputReady = true;
        await Global.SetTheme(JS, editorTheme);
        await ApplyOutputAsync();
    }

    private void OnContentChanged(ModelContentChangedEvent e)
    {
        debounceCts?.Cancel();
        var cts = debounceCts = new CancellationTokenSource();
        _ = DebouncedValidateAsync(cts.Token);
    }

    private async Task DebouncedValidateAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(400, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        await ValidateAsync();
        StateHasChanged();
    }

    private async Task ValidateAsync()
    {
        if (!editorReady)
        {
            return;
        }

        var utf16Value = await editor.GetValue();
        var options = CreateSerializerOptions();

        var start = TimeProvider.System.GetTimestamp();
        try
        {
            var document = CsTomlSerializer.Deserialize<TomlDocument>(Encoding.UTF8.GetBytes(utf16Value), options);
            lastParseMilliseconds = TimeProvider.System.GetElapsedTime(start).TotalMilliseconds;

            isValid = true;
            problems.Clear();
            await editor.ResetDeltaDecorations();

            BuildOutputs(document, options);
            await ApplyOutputAsync();
        }
        catch (CsTomlSerializeException ctse)
        {
            lastParseMilliseconds = TimeProvider.System.GetElapsedTime(start).TotalMilliseconds;

            isValid = false;
            problems.Clear();
            if (ctse.ParseExceptions is { Count: > 0 } parseExceptions)
            {
                foreach (var e in parseExceptions)
                {
                    problems.Add((e.LineNumber, e.InnerException?.Message ?? e.Message));
                }
            }
            else
            {
                problems.Add((0, ctse.InnerException?.Message ?? ctse.Message));
            }

            await DecorateErrorsAsync();
        }
        catch (Exception e)
        {
            lastParseMilliseconds = TimeProvider.System.GetElapsedTime(start).TotalMilliseconds;

            isValid = false;
            problems.Clear();
            problems.Add((0, e.Message));
            await editor.ResetDeltaDecorations();
        }
    }

    private void BuildOutputs(TomlDocument document, CsTomlSerializerOptions options)
    {
        try
        {
            jsonOutput = TomlJsonConverter.ToJson(document);
        }
        catch (Exception e)
        {
            jsonOutput = $"// Failed to convert to JSON: {e.Message}";
        }

        try
        {
            using var result = CsTomlSerializer.Serialize(document, options);
            tomlOutput = Encoding.UTF8.GetString(result.ByteSpan);
        }
        catch (Exception e)
        {
            tomlOutput = $"# Failed to serialize back to TOML: {e.Message}";
        }
    }

    private async Task ApplyOutputAsync()
    {
        if (!outputReady)
        {
            return;
        }

        var model = await outputEditor.GetModel();
        await Global.SetModelLanguage(JS, model, outputTab == "json" ? "json" : "toml");
        await outputEditor.SetValue(outputTab == "json" ? jsonOutput : tomlOutput);
    }

    private async Task DecorateErrorsAsync()
    {
        await editor.ResetDeltaDecorations();

        var decorations = problems
            .Where(p => p.Line >= 1)
            .Select(p => new ModelDeltaDecoration
            {
                Range = new BlazorMonaco.Range((int)p.Line, 1, (int)p.Line, 1),
                Options = new ModelDecorationOptions
                {
                    IsWholeLine = true,
                    ClassName = "cstoml-error-line",
                    GlyphMarginClassName = "cstoml-error-glyph",
                },
            })
            .ToArray();

        if (decorations.Length > 0)
        {
            await editor.DeltaDecorations(null, decorations);
        }
    }

    // Selects which pane is visible on narrow viewports (the pg-view-* class is
    // inert on desktop where all panes are shown by the splitter).
    private void SetMobileView(string view)
    {
        mobileView = view;
    }

    private async Task SwitchOutputTabAsync(string tab)
    {
        outputTab = tab;
        await ApplyOutputAsync();
    }

    private async Task GoToLineAsync(long line)
    {
        if (line < 1)
        {
            return;
        }

        await editor.RevealLineInCenter((int)line);
        await editor.SetPosition(new Position { LineNumber = (int)line, Column = 1 }, "cstoml-playground");
        await editor.Focus();
    }

    private async Task OnSampleChangedAsync(string? key)
    {
        if (key is null || key == selectedSample)
        {
            return;
        }

        selectedSample = key;
        var sample = TomlSamples.All.FirstOrDefault(s => s.Key == key);
        if (sample is not null)
        {
            fileUploadError = null;
            await editor.SetValue(sample.Content);
        }
    }

    private async Task OnSpecChangedAsync(string? value)
    {
        selectedSpec = value ?? MinimumVersion;
        ApplySpecPreset();
        await ValidateAsync();
    }

    private void ApplySpecPreset()
    {
        var spec = selectedSpec == "1.1.0" ? TomlSpec.Version110 : TomlSpec.Version100;
        allowUnicodeInBareKeys = spec.AllowUnicodeInBareKeys;
        allowNewlinesInInlineTables = spec.AllowNewlinesInInlineTables;
        allowTrailingCommaInInlineTables = spec.AllowTrailingCommaInInlineTables;
        allowSecondsOmissionInTime = spec.AllowSecondsOmissionInTime;
        supportsEscapeSequenceE = spec.SupportsEscapeSequenceE;
        supportsEscapeSequenceX = spec.SupportsEscapeSequenceX;
    }

    private CsTomlSerializerOptions CreateSerializerOptions()
    {
        // The feature toggles are always effective; the spec selector only presets them.
        var anyFeatureEnabled = allowUnicodeInBareKeys || allowNewlinesInInlineTables
            || allowTrailingCommaInInlineTables || allowSecondsOmissionInTime
            || supportsEscapeSequenceE || supportsEscapeSequenceX;
        if (!anyFeatureEnabled)
        {
            return CsTomlSerializerOptions.Default;
        }

        return CsTomlSerializerOptions.Default with
        {
            Spec = new()
            {
                AllowUnicodeInBareKeys = allowUnicodeInBareKeys,
                AllowNewlinesInInlineTables = allowNewlinesInInlineTables,
                AllowTrailingCommaInInlineTables = allowTrailingCommaInInlineTables,
                AllowSecondsOmissionInTime = allowSecondsOmissionInTime,
                SupportsEscapeSequenceE = supportsEscapeSequenceE,
                SupportsEscapeSequenceX = supportsEscapeSequenceX,
            },
        };
    }

    private async Task RevalidateAsync()
    {
        await ValidateAsync();
    }

    private async Task ValidateNowAsync()
    {
        debounceCts?.Cancel();
        await ValidateAsync();
    }

    private async Task OpenFilePickerAsync()
    {
        await JS.InvokeVoidAsync("csTomlPlayground.clickElement", "pg-file-input");
    }

    private async Task OnFileSelectedAsync(InputFileChangeEventArgs e)
    {
        var file = e.File;

        if (file.Size > MaxFileSize)
        {
            fileUploadError = "File size exceeds 1MB limit.";
            return;
        }

        fileUploadError = null;

        try
        {
            using var stream = file.OpenReadStream(maxAllowedSize: MaxFileSize);
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            await editor.SetValue(content);
        }
        catch (Exception ex)
        {
            fileUploadError = $"File read error: {ex.Message}";
        }
    }

    private async Task ClearEditorAsync()
    {
        fileUploadError = null;
        await editor.SetValue(string.Empty);
    }

    private async Task CopyOutputAsync()
    {
        var text = outputTab == "json" ? jsonOutput : tomlOutput;
        await JS.InvokeVoidAsync("csTomlPlayground.copyText", text);

        copied = true;
        StateHasChanged();
        await Task.Delay(1500);
        copied = false;
    }

    private async Task SyncEditorThemeToAppThemeAsync()
    {
        editorTheme = isDarkMode ? "vs-dark" : "vs";
        await Global.SetTheme(JS, editorTheme);
    }

    private async Task ToggleThemeAsync()
    {
        isDarkMode = !isDarkMode;
        if (ThemeService is not null)
        {
            await ThemeService.SetThemeAsync(isDarkMode ? ThemeMode.Dark : ThemeMode.Light);
        }
        await SyncEditorThemeToAppThemeAsync();
    }

    public void Dispose()
    {
        debounceCts?.Cancel();
        debounceCts?.Dispose();
    }
}
