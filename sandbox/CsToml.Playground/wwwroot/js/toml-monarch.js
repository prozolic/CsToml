// Registers a TOML language definition (Monarch tokenizer) with the Monaco editor.
// Monaco does not ship TOML support out of the box, so the playground provides its own.
(function () {
    function registerToml() {
        if (!window.monaco || !monaco.languages) {
            return;
        }
        if (monaco.languages.getLanguages().some(function (l) { return l.id === "toml"; })) {
            return;
        }

        monaco.languages.register({ id: "toml", extensions: [".toml"], aliases: ["TOML", "toml"] });

        monaco.languages.setLanguageConfiguration("toml", {
            comments: { lineComment: "#" },
            brackets: [["[", "]"], ["{", "}"]],
            autoClosingPairs: [
                { open: "[", close: "]" },
                { open: "{", close: "}" },
                { open: "\"", close: "\"", notIn: ["string"] },
                { open: "'", close: "'", notIn: ["string"] }
            ],
            surroundingPairs: [
                { open: "[", close: "]" },
                { open: "{", close: "}" },
                { open: "\"", close: "\"" },
                { open: "'", close: "'" }
            ]
        });

        monaco.languages.setMonarchTokensProvider("toml", {
            defaultToken: "",
            tokenPostfix: ".toml",

            // A bare-key segment (permissive so that unicode bare keys also highlight), or a quoted key.
            keySegment: /(?:[^\s=.#\[\]{}"',]+|"[^"\\]*(?:\\.[^"\\]*)*"|'[^']*')/,

            escapes: /\\(?:[btnfr"\\eE]|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8})/,

            tokenizer: {
                root: [
                    // Comments.
                    [/#.*$/, "comment"],

                    // Table headers: [table] and [[array of tables]] (must close on the same line).
                    [/^(\s*)(\[\[)(?=[^\]]*\]\]\s*(?:#.*)?$)/, ["", { token: "keyword", next: "@header" }]],
                    [/^(\s*)(\[)(?=[^\]]*\]\s*(?:#.*)?$)/, ["", { token: "keyword", next: "@header" }]],

                    // Keys (including dotted keys and keys inside inline tables).
                    [/@keySegment(?:\s*\.\s*@keySegment)*(?=\s*=)/, "type"],

                    // Strings.
                    [/"""/, { token: "string", next: "@mlstring" }],
                    [/"/, { token: "string", next: "@string" }],
                    [/'''/, { token: "string", next: "@mlliteral" }],
                    [/'[^']*'/, "string"],

                    // Date / time (before numbers).
                    [/\d{4}-\d{2}-\d{2}(?:[Tt ]\d{2}:\d{2}(?::\d{2})?(?:\.\d+)?(?:[Zz]|[+-]\d{2}:\d{2})?)?/, "number.date"],
                    [/\d{2}:\d{2}(?::\d{2})?(?:\.\d+)?/, "number.date"],

                    // Numbers.
                    [/[+-]?(?:inf|nan)\b/, "number.float"],
                    [/0x[0-9A-Fa-f](?:[0-9A-Fa-f_]*[0-9A-Fa-f])?/, "number.hex"],
                    [/0o[0-7](?:[0-7_]*[0-7])?/, "number.octal"],
                    [/0b[01](?:[01_]*[01])?/, "number.binary"],
                    [/[+-]?\d(?:[\d_]*\d)?(?:\.\d(?:[\d_]*\d)?)?[eE][+-]?\d(?:[\d_]*\d)?/, "number.float"],
                    [/[+-]?\d(?:[\d_]*\d)?\.\d(?:[\d_]*\d)?/, "number.float"],
                    [/[+-]?\d(?:[\d_]*\d)?/, "number"],

                    // Booleans.
                    [/\b(?:true|false)\b/, "keyword"],

                    // Delimiters.
                    [/[=.,]/, "delimiter"],
                    [/[\[\]{}]/, "@brackets"]
                ],

                header: [
                    [/[^\]]+/, "keyword"],
                    [/\]\]?/, { token: "keyword", next: "@pop" }]
                ],

                string: [
                    [/[^\\"]+/, "string"],
                    [/@escapes/, "string.escape"],
                    [/\\./, "string.escape.invalid"],
                    [/"/, { token: "string", next: "@pop" }]
                ],

                mlstring: [
                    [/[^\\"]+/, "string"],
                    [/@escapes/, "string.escape"],
                    [/\\$/, "string.escape"],
                    [/\\./, "string.escape.invalid"],
                    [/"""/, { token: "string", next: "@pop" }],
                    [/"/, "string"]
                ],

                mlliteral: [
                    [/[^']+/, "string"],
                    [/'''/, { token: "string", next: "@pop" }],
                    [/'/, "string"]
                ]
            }
        });
    }

    // monaco is materialized lazily by the AMD loader, so the global may not exist yet
    // when this script runs. Prefer the AMD require; fall back to polling.
    if (window.monaco && monaco.languages) {
        registerToml();
    } else if (typeof require === "function") {
        require(["vs/editor/editor.main"], registerToml);
    } else {
        var attempts = 0;
        var timer = setInterval(function () {
            if (window.monaco && monaco.languages) {
                clearInterval(timer);
                registerToml();
            } else if (++attempts > 400) {
                clearInterval(timer);
                console.warn("monaco was not loaded; TOML language registration skipped.");
            }
        }, 50);
    }
})();
