using System.Collections.Immutable;

namespace CsToml.Playground.Models;

public sealed record TomlSample(string Key, string DisplayName, string Content);

public static class TomlSamples
{
    public static readonly ImmutableArray<TomlSample> All =
    [
        new("basic", "Basic example",
            """
            # This is a TOML document

            title = "TOML Example"

            [owner]
            name = "Tom Preston-Werner"
            dob = 1979-05-27T07:32:00-08:00

            [database]
            enabled = true
            ports = [ 8000, 8001, 8002 ]
            data = [ ["delta", "phi"], [3.14] ]
            temp_targets = { cpu = 79.5, case = 72.0 }

            [servers]

            [servers.alpha]
            ip = "10.0.0.1"
            role = "frontend"

            [servers.beta]
            ip = "10.0.0.2"
            role = "backend"
            """),

        new("string", "String",
            """"
            # Basic strings support escape sequences.
            str1 = "I'm a string. \"You can quote me\". Tab\there."

            # Multi-line basic strings trim the first newline.
            str2 = """
            Roses are red
            Violets are blue"""

            # A trailing \ trims the following whitespace and newline.
            str3 = """\
                   The quick brown \
                   fox jumps over \
                   the lazy dog.\
                   """

            # Literal strings: no escaping at all.
            winpath  = 'C:\Users\nodejs\templates'
            winpath2 = '\\ServerX\admin$\system32\'
            quoted   = 'Tom "Dubs" Preston-Werner'
            regex    = '<\i\c*\s*>'

            # Multi-line literal strings.
            regex2 = '''I [dw]on't need \d{2} apples'''
            lines  = '''
            The first newline is
            trimmed in raw strings.
               All other whitespace
               is preserved.
            '''
            """"),

        new("integer", "Integer",
            """
            int1 = +99
            int2 = 42
            int3 = 0
            int4 = -17

            # Underscores enhance readability.
            int5 = 1_000
            int6 = 5_349_221
            int7 = 53_49_221  # Indian number system grouping
            int8 = 1_2_3_4_5  # VALID but discouraged

            # Hexadecimal with prefix `0x`.
            hex1 = 0xDEADBEEF
            hex2 = 0xdeadbeef
            hex3 = 0xdead_beef

            # Octal with prefix `0o`.
            oct1 = 0o01234567
            oct2 = 0o755  # useful for Unix file permissions

            # Binary with prefix `0b`.
            bin1 = 0b11010110
            """),

        new("float", "Float",
            """
            # Fractional.
            flt1 = +1.0
            flt2 = 3.1415
            flt3 = -0.01

            # Exponent.
            flt4 = 5e+22
            flt5 = 1e06
            flt6 = -2E-2

            # Both.
            flt7 = 6.626e-34

            # Underscores enhance readability.
            flt8 = 224_617.445_991_228

            # Infinity.
            sf1 = inf   # positive infinity
            sf2 = +inf  # positive infinity
            sf3 = -inf  # negative infinity

            # Not a number.
            sf4 = nan
            sf5 = +nan
            sf6 = -nan
            """),

        new("boolean", "Boolean",
            """
            bool1 = true
            bool2 = false
            """),

        new("offset-date-time", "Offset Date-Time",
            """
            # RFC 3339 date-time with offset.
            odt1 = 1979-05-27T07:32:00Z
            odt2 = 1979-05-27T00:32:00-07:00
            odt3 = 1979-05-27T00:32:00.999999-07:00

            # The T delimiter may be replaced by a space.
            odt4 = 1979-05-27 07:32:00Z
            """),

        new("local-date-time", "Local Date-Time",
            """
            # Date-time without any offset.
            ldt1 = 1979-05-27T07:32:00
            ldt2 = 1979-05-27T00:32:00.999999
            """),

        new("local-date", "Local Date",
            """
            # A calendar date without time and offset.
            ld1 = 1979-05-27
            """),

        new("local-time", "Local Time",
            """
            # A time of day without date and offset.
            lt1 = 07:32:00
            lt2 = 00:32:00.999999

            # With the TOML v1.1.0 spec, seconds may be omitted
            # (enable "Seconds omission in time" in Options):
            #lt3 = 07:32
            """),

        new("array", "Array",
            """"
            integers = [ 1, 2, 3 ]
            colors = [ "red", "yellow", "green" ]
            nested_arrays_of_ints = [ [ 1, 2 ], [3, 4, 5] ]
            nested_mixed_array = [ [ 1, 2 ], ["a", "b", "c"] ]
            string_array = [ "all", 'strings', """are the same""", '''type''' ]

            # Mixed-type arrays are allowed.
            numbers = [ 0.1, 0.2, 0.5, 1, 2, 5 ]
            contributors = [
              "Foo Bar <foo@example.com>",
              { name = "Baz Qux", email = "bazqux@example.com", url = "https://example.com/bazqux" }
            ]

            # Line breaks are OK; a terminating comma is also allowed.
            integers2 = [
              1,
              2,
              3,
            ]
            """"),

        new("table", "Table",
            """
            # Dotted keys create and define a table for each key part before the last one.
            fruit.apple.color = "red"
            fruit.apple.taste.sweet = true

            [table-1]
            key1 = "some string"
            key2 = 123

            [table-2]
            key1 = "another string"
            key2 = 456

            # Quoted keys may contain any character.
            [dog."tater.man"]
            type.name = "pug"

            # Whitespace around dotted parts is ignored.
            [a.b.c]            # this is best practice
            [ d.e.f ]          # same as [d.e.f]
            [ g .  h  . i ]    # same as [g.h.i]
            [ j . "k" . 'l' ]  # same as [j."k".'l']
            """),

        new("inline-table", "Inline Table",
            """
            name = { first = "Tom", last = "Preston-Werner" }
            point = { x = 1, y = 2 }
            animal = { type.name = "pug" }

            # With the TOML v1.1.0 spec, newlines and trailing commas
            # are also allowed inside inline tables (see Options).
            """),

        new("array-of-tables", "Array of Tables",
            """
            [[products]]
            name = "Hammer"
            sku = 738594937

            [[products]]  # empty table within the array

            [[products]]
            name = "Nail"
            sku = 284758393
            color = "gray"

            [[fruits]]
            name = "apple"

            [fruits.physical]  # subtable
            color = "red"
            shape = "round"

            [[fruits.varieties]]  # nested array of tables
            name = "red delicious"

            [[fruits.varieties]]
            name = "granny smith"

            [[fruits]]
            name = "banana"

            [[fruits.varieties]]
            name = "plantain"
            """),

        new("v110", "TOML v1.1.0 features",
            """
            # Requires the TOML v1.1.0 spec (Options panel).
            # Enable "Unicode in bare keys" as well to allow the last table.

            [strings]
            escape_e = "\e[32mgreen\e[0m"       # \e escape sequence
            escape_x = "\x1b[1mbold\x1b[0m"     # \x hexadecimal escape

            [times]
            seconds_omitted = 09:30             # seconds omission in time

            [inline]
            multiline = {
              name = "newlines in inline tables",
              trailing = "comma below is allowed",
            }
            """),
    ];
}
