using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Ketexon.YarnUITK
{
    //public static class VisualElementSelectorParser
    //{
    //    public enum IdentType
    //    {
    //        Name,
    //        Class,
    //    }

    //    public enum TokenType
    //    {
    //        Period,
    //        Comma,
    //        Hashtag,
    //        Whitespace,
    //        GT,
    //        Colon,
    //        Star,
    //        Ident,
    //        Unknown
    //    }

    //    public struct Token
    //    {
    //        public TokenType Type;
    //        public string StringValue;

    //        public static implicit operator Token((TokenType, string) tuple) => new Token { Type = tuple.Item1, StringValue = tuple.Item2 };
    //    }

    //    public static bool IsValidIdentifierChar(char c)
    //    {
    //        return
    //            char.IsLetter(c)
    //            || char.IsDigit(c)
    //            || c == '_';
    //    }

    //    public static bool IsValidIdentifierStart(char c)
    //    {
    //        return
    //            char.IsLetter(c)
    //            || c == '_';
    //    }

    //    public static List<Token> Lex(string selector)
    //    {
    //        List<Token> list = new List<Token>();

    //        int cursor = 0;
    //        while (cursor < selector.Length)
    //        {
    //            var c = selector[cursor];
    //            if (c == '.')
    //            {
    //                list.Add((TokenType.Period, "."));
    //            }
    //            else if (c == ',')
    //            {
    //                list.Add((TokenType.Period, ","));
    //            }
    //            else if (c == '#')
    //            {
    //                list.Add((TokenType.Hashtag, "#"));
    //            }
    //            else if (char.IsWhiteSpace(c))
    //            {
    //                var whiteSpace = default(string);
    //                while (cursor < selector.Length && char.IsWhiteSpace(selector[cursor]))
    //                {
    //                    whiteSpace += selector[cursor];
    //                    cursor++;
    //                }
    //                cursor--; // end on last whitespace character
    //                list.Add((TokenType.Whitespace, whiteSpace));
    //            }
    //            else if (c == '>')
    //            {
    //                list.Add((TokenType.GT, ">"));
    //            }
    //            else if (c == ':')
    //            {
    //                list.Add((TokenType.Colon, ":"));
    //            }
    //            else if (c == '*')
    //            {
    //                list.Add((TokenType.Star, "*"));
    //            }
    //            else if (IsValidIdentifierStart(c))
    //            {
    //                var ident = default(string);
    //                ident += c;
    //                cursor++;

    //                while (cursor < selector.Length && IsValidIdentifierChar(selector[cursor]))
    //                {
    //                    ident += selector[cursor];
    //                    cursor++;
    //                }
    //                cursor--;
    //                list.Add((TokenType.Ident, ident));
    //            }
    //            else
    //            {
    //                list.Add((TokenType.Unknown, c.ToString()));
    //            }

    //            cursor++;
    //        }

    //        return list;
    //    }
    //}

    //[System.Serializable]
    //public class VisualElementSelector
    //{
    //    public class InvalidSyntaxException : System.Exception
    //    {
    //        public InvalidSyntaxException() { }
    //        public InvalidSyntaxException(string s) : base(s) { }
    //        public InvalidSyntaxException(string s, System.Exception inner) : base(s, inner) { }
    //    }

    //    private enum SelectorCombiner
    //    {
    //        Comma,
    //    }

    //    public string Selector;

    //    public VisualElementSelector(string selector)
    //    {
    //        Selector = selector;
    //    }

    //    public UQueryBuilder<T> Builder<T>(VisualElement visualElement)
    //        where T : VisualElement
    //    {
    //        var builder = new UQueryBuilder<T>(visualElement);

    //        var tokens = VisualElementSelectorParser.Lex(Selector);
    //        var cursor = 0;
    //        var expectingSelector = false;
    //        var expectingSelectorAfterWhat = default(string);
    //        var whiteSpaceImmediatelyBefore = false;

    //        while (cursor < tokens.Count)
    //        {
    //            var thisIterIsWhiteSpace = false;
    //            var token = tokens[cursor];
    //            if(token.Type == VisualElementSelectorParser.TokenType.Whitespace)
    //            {
    //                cursor++;
    //                while(cursor < tokens.Count && tokens[cursor].Type == VisualElementSelectorParser.TokenType.Whitespace)
    //                {
    //                    cursor++;
    //                }
    //                thisIterIsWhiteSpace = true;
    //            }
    //            else if (token.Type == VisualElementSelectorParser.TokenType.Period // class selector
    //                || token.Type == VisualElementSelectorParser.TokenType.Hashtag // name selector
    //            )
    //            {
    //                cursor++;
    //                var ident = tokens[cursor];
    //                if(ident.Type != VisualElementSelectorParser.TokenType.Ident)
    //                {
    //                    throw new InvalidSyntaxException("Periods and Hashtags must be followe by an identifier");
    //                }
    //                if(token.Type == VisualElementSelectorParser.TokenType.Period) // class selector
    //                {
    //                    builder.Class(ident.StringValue);
    //                }
    //                else
    //                {
    //                    builder.Name(ident.StringValue);
    //                }
    //                expectingSelector = false; // fulfilled
    //            }
    //            else if (expectingSelector)
    //            {
    //                throw new System.NotImplementedException($"Expected selector after {expectingSelectorAfterWhat}.");
    //            }
    //            else if(token.Type == VisualElementSelectorParser.TokenType.Ident) // type selector
    //            {
    //                throw new System.NotImplementedException("Type selectors are not implemented.");
    //            }
    //            else if(token.Type == VisualElementSelectorParser.TokenType.Comma)
    //            {
    //                cursor++;
    //                while(cursor < tokens.Count && tokens[cursor].Type == VisualElementSelectorParser.TokenType.Whitespace)
    //                {
    //                    cursor++; // pass through whitespace
    //                }
    //                if(cursor >= tokens.Count)
    //                {
    //                    throw new InvalidSyntaxException("Comma must be followed by a class selector, found end of string.");
    //                }
    //                var ident = tokens[cursor];
    //                if(
    //                    ident.Type != VisualElementSelectorParser.TokenType.Period
    //                )
    //                {
    //                    throw new InvalidSyntaxException($"Comma must be followed by a class selector, found {ident.Type}");
    //                }
    //                cursor--;
    //                // default behavior is adding a new class to class list, so we can just continue
    //            }
    //            else if(token.Type == VisualElementSelectorParser.TokenType.GT)
    //            {

    //            }

    //            whiteSpaceImmediatelyBefore = thisIterIsWhiteSpace;

    //            cursor++;
    //        }

    //        return builder;
    //    }
    //}

    /// <summary>
    /// Supports one class/name selector with pseudoselectors (no whitespace)
    /// </summary>
    [System.Serializable]
    public class BasicStringVisualElementSelector
    {
        public string Selector;

        public BasicStringVisualElementSelector(string selector)
        {
            Selector = selector;
        }

        private static bool IsValidStartIdent(char c) => char.IsLetter(c) || c == '_' || c == '-';
        private static bool IsValidContinueIdent(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '-';

        public UQueryBuilder<T> Builder<T>(VisualElement visualElement = null)
            where T : VisualElement
        {
            UQueryBuilder<T> builder;
            if (visualElement == null)
            {
                builder = new UQueryBuilder<T>();
            }
            else
            {
                builder = new UQueryBuilder<T>(visualElement);
            }
            var cursor = 0;
            if(Selector.Length <= 1)
            {
                throw new System.ArgumentException("Selector must be at least 2 characters long");
            }
            if (Selector[cursor] != '.' && Selector[cursor] != '#')
            {
                throw new System.ArgumentException("Selector must start with '.' or '#'");
            }
            var classSelector = Selector[cursor] == '.';
            cursor++;

            
            if (!IsValidStartIdent(Selector[cursor]))
            {
                throw new System.ArgumentException("Selector identifier must start with a letter or underscore");
            }
            var ident = default(string);
            ident += Selector[cursor];
            cursor++;
            while(cursor < Selector.Length && Selector[cursor] != ':')
            {
                if (!IsValidContinueIdent(Selector[cursor]))
                {
                    throw new System.ArgumentException($"Unknown character in ident: '{Selector[cursor]}'");
                }
                ident += Selector[cursor];
                cursor++;
            }


            if (classSelector)
            {
                builder.Class(ident);
            }
            else
            {
                builder.Name(ident);
            }

            // Pseudoclasses
            while(cursor < Selector.Length)
            {
                if(Selector[cursor] == ':')
                {
                    cursor++;
                    ident = "";
                    while(cursor < Selector.Length && char.IsLetter(Selector[cursor]))
                    {
                        ident += Selector[cursor];
                        cursor++;
                    }
                    switch (ident)
                    {
                        case "hover":
                            builder.Hovered();
                            break;
                        case "active":
                            builder.Active();
                            break;
                        case "inactive":
                            builder.NotActive();
                            break;
                        case "focus":
                            builder.Focused();
                            break;
                        case "selected":
                            builder.Checked();
                            break;
                        case "disabled":
                            builder.NotEnabled();
                            break;
                        case "enabled":
                            builder.Enabled();
                            break;
                        case "checked":
                            builder.Checked();
                            break;
                        case "root":
                            throw new System.NotImplementedException("Cannot use pseudoselector ':root'");
                        default:
                            throw new System.ArgumentException($"Unknown pseudoselector: ':{ident}'");
                    }
                    if(cursor < Selector.Length)
                    {
                        cursor--;
                    }
                }
                else
                {
                    throw new System.ArgumentException($"Unknown character in pseudoselector: '{Selector[cursor]}'");
                }
                cursor++;
            }

            return builder;
        }
    }

    [System.Serializable]
    public class BasicVisualElementSelector
    {
        [System.Flags]
        public enum PseudoSelector
        {
            None = 0,
            Hover = 1 << 0,
            Active = 1 << 1,
            Inactive = 1 << 2,
            Focus = 1 << 3,
            Disabled = 1 << 4,
            Enabled = 1 << 5,
            Checked = 1 << 6
        }

        public string Name = "";
        public List<string> Classes = new List<string>();

        public PseudoSelector PseudoSelectorMask = PseudoSelector.None;

        public UQueryBuilder<T> Builder<T>(VisualElement visualElement = null)
            where T : VisualElement
        {

            UQueryBuilder<T> builder;
            if(visualElement != null)
            {
                builder = new UQueryBuilder<T>(visualElement);
            }
            else
            {
                builder = new UQueryBuilder<T>();
            }

            if(Name != "")
            {
                builder.Name(Name);
            }

            foreach(var clazz in Classes){
                builder.Class(clazz);
            }

            if (PseudoSelectorMask.HasFlag(PseudoSelector.Hover))
            {
                builder.Hovered();
            }
            if (PseudoSelectorMask.HasFlag(PseudoSelector.Active))
            {
                builder.Active();
            }
            if(PseudoSelectorMask.HasFlag(PseudoSelector.Inactive))
            {
                builder.NotActive();
            }
            if (PseudoSelectorMask.HasFlag(PseudoSelector.Active))
            {
                builder.Active();
            }
            if (PseudoSelectorMask.HasFlag(PseudoSelector.Focus))
            {
                builder.Focused();
            }
            if (PseudoSelectorMask.HasFlag(PseudoSelector.Disabled))
            {
                builder.NotEnabled();
            }
            if (PseudoSelectorMask.HasFlag(PseudoSelector.Enabled))
            {
                builder.Enabled();
            }

            return builder;
        }
    }
}
