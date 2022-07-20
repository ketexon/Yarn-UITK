using System.Collections.Generic;

namespace Ketexon.YarnUITK
{
    class RichText
    {
        public class Tag
        {
            public readonly string StartText;
            public readonly string EndText;

            /// <summary>
            /// Inclusive
            /// </summary>
            public readonly int StartIndex;

            /// <summary>
            /// Exclusive
            /// </summary>
            public readonly int EndIndex;

            public static string Escape(string tag)
            {
                if (tag.Length < 3)
                {
                    throw new System.ArgumentException("tag must be at least 3 characters long");
                }
                if (tag[0] != '<')
                {
                    throw new System.ArgumentException("tag must start with <");
                }
                if (tag[^1] != '>')
                {
                    throw new System.ArgumentException("tag must end with >");
                }
                return "<" + "<b></b>" + tag.Substring(1);
            }

            public Tag(string startText, string endText, int startIndex, int endIndex)
            {
                StartText = startText;
                EndText = endText;
                StartIndex = startIndex;
                EndIndex = endIndex;
            }

            public override string ToString()
            {
                return $"{base.ToString()} {Escape(StartText)}...{Escape(EndText)} [{StartIndex}, {EndIndex})";
            }
        }

        public readonly string Text;
        public readonly string PlainText = "";
        public readonly List<Tag> Tags = new List<Tag>();


        public RichText(string richText)
        {
            Text = richText;

            Stack<(int, string)> tagStarts = new Stack<(int, string)>();

            int plainTextIndex = 0;
            for (int i = 0; i < richText.Length; i++)
            {
                var c = Text[i];
                if (c == '<')
                {
                    i++;
                    c = Text[i];
                    if (c != '/') // open tag
                    {
                        var potentialTag = "<";
                        var tagValid = false;

                        while (i < richText.Length)
                        {
                            c = Text[i];
                            if (c == '>') // found closing bracket
                            {
                                potentialTag += '>';
                                if (potentialTag.Length > 2)
                                {
                                    tagValid = true;
                                    // hi <b>there</b> friend
                                    // 012   3456789...
                                    //       ^ plaintext index here
                                    tagStarts.Push((plainTextIndex, potentialTag));
                                    break;
                                }
                                else
                                {
                                    tagValid = false;
                                    break;
                                }
                            }
                            else if (c == '<') // found another open bracket before closing
                            {
                                tagValid = false;
                                i--;
                                break;
                            }
                            else
                            {
                                potentialTag += c;
                            }
                            i++;
                        }
                        if (!tagValid)
                        {
                            PlainText += potentialTag;
                        }
                    }
                    else // closing tag
                    {
                        i++;
                        var potentialTag = "</";
                        var tagValid = false;

                        while (i < richText.Length)
                        {
                            c = Text[i];
                            if (c == '>') // found closing bracket
                            {
                                potentialTag += '>';
                                if (potentialTag.Length > 3)
                                {
                                    (int, string) startTag;
                                    if (tagStarts.TryPop(out startTag))
                                    {
                                        // hi <b>there</b> friend
                                        // 012   345678   9...
                                        //                ^ plaintext index here
                                        tagValid = true;
                                        Tags.Add(
                                            new Tag(
                                                startText: startTag.Item2,
                                                endText: potentialTag,
                                                startIndex: startTag.Item1,
                                                endIndex: plainTextIndex
                                            )
                                        );
                                    }
                                    else // no corresponding starttag
                                    {
                                        tagValid = false;
                                    }
                                    break;
                                }
                                else
                                {
                                    tagValid = false;
                                    break;
                                }
                            }
                            else if (c == '<') // found another open bracket before closing
                            {
                                tagValid = false;
                                i--;
                                break;
                            }
                            else
                            {
                                potentialTag += c;
                            }
                            i++;
                        }
                        if (!tagValid)
                        {
                            PlainText += potentialTag;
                        }
                    }
                }
                else
                {
                    PlainText += c;
                    plainTextIndex++;
                }
            }
        }

        public RichText Substring(int startIndex = 0, int length = -1)
        {
            var endIndex = default(int);
            if (length < 0)
            {
                endIndex = PlainText.Length;
            }
            else
            {
                endIndex = startIndex + length;
                if (endIndex > PlainText.Length)
                {
                    throw new System.ArgumentException("Length exceeds length of PlainText");
                }
            }

            string outText = "";
            List<Tag> activeTags = new List<Tag>(Tags);
            Stack<Tag> startedTags = new Stack<Tag>();

            var tagsBeforeStart = new List<Tag>();
            foreach (var tag in activeTags)
            {
                if (tag.StartIndex < startIndex && tag.EndIndex > startIndex)
                {
                    tagsBeforeStart.Add(tag);
                }
            }
            tagsBeforeStart.Sort(
                (t1, t2) =>
                {
                    var startCompare = Comparer<int>.Default.Compare(t1.StartIndex, t2.StartIndex);
                    if (startCompare == 0)
                    {
                        return -Comparer<int>.Default.Compare(t1.EndIndex, t2.EndIndex);
                    }
                    else
                    {
                        return startCompare;
                    }
                }
            );
            foreach (var tag in tagsBeforeStart)
            {
                outText += tag.StartText;
                startedTags.Push(tag);
            }

            for (int i = startIndex; i < endIndex; i++)
            {
                var startTagsAtIndex = new List<Tag>();
                foreach (var tag in activeTags)
                {
                    if (tag.StartIndex == i)
                    {
                        startTagsAtIndex.Add(tag);
                    }
                }
                startTagsAtIndex.Sort( // shorter end index comes last
                    (t1, t2) => -Comparer<int>.Default.Compare(t1.EndIndex, t2.EndIndex)
                );
                foreach (var tag in startTagsAtIndex)
                {
                    //Debug.Log(tag);
                    outText += tag.StartText;
                    startedTags.Push(tag);
                }
                Tag topStartedTag;
                while (startedTags.TryPeek(out topStartedTag) && topStartedTag.EndIndex == i)
                {
                    outText += topStartedTag.EndText;
                    activeTags.Remove(startedTags.Pop());
                }

                outText += PlainText[i];
            }
            foreach (var tag in startedTags)
            {
                outText += tag.EndText;
            }
            return new RichText(outText);
        }

        public string Escape()
        {
            string outText = "";
            List<Tag> activeTags = new List<Tag>(Tags);
            Stack<Tag> startedTags = new Stack<Tag>();
            for (int i = 0; i < PlainText.Length; i++)
            {
                var startTagsAtIndex = new List<Tag>();
                foreach (var tag in activeTags)
                {
                    if (tag.StartIndex == i)
                    {
                        startTagsAtIndex.Add(tag);
                    }
                }
                startTagsAtIndex.Sort(
                    (t1, t2) => -Comparer<int>.Default.Compare(t1.EndIndex, t2.EndIndex)
                );
                foreach (var tag in startTagsAtIndex)
                {
                    outText += Tag.Escape(tag.StartText);
                    startedTags.Push(tag);
                }
                Tag topStartedTag;
                while (startedTags.TryPeek(out topStartedTag) && topStartedTag.EndIndex == i)
                {
                    outText += Tag.Escape(topStartedTag.EndText);
                    activeTags.Remove(startedTags.Pop());
                }

                outText += PlainText[i];
            }
            foreach (var tag in startedTags)
            {
                outText += tag.EndText;
            }
            return outText;
        }
    }
}
