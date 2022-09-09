using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Yarn.Unity;

namespace Ketexon.YarnUITK
{
    /// <summary>
    /// Contains coroutine methods that apply visual effects. This class is used
    /// by <see cref="LineView"/> to handle animating the presentation of lines.
    /// </summary>
    public static class Effects
    {
        /// <summary>
        /// An object that can be used to signal to a coroutine that it should
        /// terminate early.
        /// </summary>
        /// <remarks>
        /// <para>
        /// While coroutines can be stopped by calling <see
        /// cref="MonoBehaviour.StopCoroutine"/> or <see
        /// cref="MonoBehaviour.StopAllCoroutines"/>, this has the side effect
        /// of also stopping any coroutine that was waiting for the now-stopped
        /// coroutine to finish.
        /// </para>
        /// <para>
        /// Instances of this class may be passed as a parameter to a coroutine
        /// that they can periodically poll to see if they should terminate
        /// earlier than planned.
        /// </para>
        /// <para>
        /// To use this class, create an instance of it, and pass it as a
        /// parameter to your coroutine. In the coroutine, call <see
        /// cref="Start"/> to mark that the coroutine is running. During the
        /// coroutine's execution, periodically check the <see
        /// cref="WasInterrupted"/> property to determine if the coroutine
        /// should exit. If it is <see langword="true"/>, the coroutine should
        /// exit (via the <c>yield break</c> statement.) At the normal exit of
        /// your coroutine, call the <see cref="Complete"/> method to mark that the
        /// coroutine is no longer running. To make a coroutine stop, call the
        /// <see cref="Interrupt"/> method.
        /// </para>
        /// <para>
        /// You can also use the <see cref="CanInterrupt"/> property to
        /// determine if the token is in a state in which it can stop (that is,
        /// a coroutine that's using it is currently running.)
        /// </para>
        /// </remarks>
        public class CoroutineInterruptToken
        {

            /// <summary>
            /// The state that the token is in.
            /// </summary>
            enum State
            {
                NotRunning,
                Running,
                Interrupted,
            }
            private State state = State.NotRunning;

            public bool CanInterrupt => state == State.Running;
            public bool WasInterrupted => state == State.Interrupted;
            public void Start() => state = State.Running;
            public void Interrupt()
            {
                if (CanInterrupt == false)
                {
                    throw new InvalidOperationException($"Cannot stop {nameof(CoroutineInterruptToken)}; state is {state} (and not {nameof(State.Running)}");
                }
                state = State.Interrupted;
            }

            public void Complete() => state = State.NotRunning;
        }

        /// <summary>
        /// A coroutine that gradually reveals the text in a object over time.
        /// </summary>
        /// <remarks>
        /// <para>This method works by adding a transparent color tag to the richtext to prevent a word from moving to the next line if too many characters are added.</para>
        /// <para style="note">Depending on the value of <paramref name="lettersPerSecond"/>, <paramref name="onCharacterTyped"/> may be called multiple times per frame.</para>
        /// </remarks>
        /// <param name="text">The richtext text to reveal. </param>
        /// <param name="label">The label that the richtext will be placed in. Rich text must be enabled.</param>
        /// <param name="lettersPerSecond">The number of letters that should be
        /// revealed per second.</param>
        /// <param name="onCharacterTyped">An <see cref="Action"/> that should be called for each character that was revealed.</param>
        /// <param name="stopToken">A <see cref="CoroutineInterruptToken"/> that
        /// can be used to interrupt the coroutine.</param>
        public static IEnumerator Typewriter(string text, Label label, float lettersPerSecond, Action onCharacterTyped, CoroutineInterruptToken stopToken = null)
        {
            var richText = new RichText(text);

            stopToken?.Start();

            label.text = "";

            // How many visible characters are present in the text?
            var characterCount = richText.PlainText.Length;

            // Early out if letter speed is zero, text length is zero
            if (lettersPerSecond <= 0 || characterCount == 0)
            {
                // Show everything and return
                label.text = text;
                stopToken?.Complete();
                yield break;
            }

            label.text = richText.GenerateText(
                extraTags: new List<RichText.Tag> { new RichText.Tag("<color=#00000000>", "</color>", 0, characterCount) },
                tagMap: tag => tag.StartText.StartsWith("<color")
                    ? new RichText.Tag(tag.StartText, tag.EndText, 0, 0)
                    : tag
            );

            // Convert 'letters per second' into its inverse
            float secondsPerLetter = 1.0f / lettersPerSecond;

            // If lettersPerSecond is larger than the average framerate, we
            // need to show more than one letter per frame, so simply
            // adding 1 letter every secondsPerLetter won't be good enough
            // (we'd cap out at 1 letter per frame, which could be slower
            // than the user requested.)
            //
            // Instead, we'll accumulate time every frame, and display as
            // many letters in that frame as we need to in order to achieve
            // the requested speed.
            var accumulator = Time.deltaTime;

            var charactersVisible = 0;
            while (charactersVisible < characterCount)
            {
                if (stopToken?.WasInterrupted ?? false)
                {
                    yield break;
                }

                // We need to show as many letters as we have accumulated
                // time for.
                while (accumulator >= secondsPerLetter)
                {
                    charactersVisible += 1;
                    onCharacterTyped?.Invoke();
                    accumulator -= secondsPerLetter;
                }
                accumulator += Time.deltaTime;
                var transparentTag = new RichText.Tag("<color=#00000000>", "</color>", charactersVisible, characterCount);
                label.text = richText.GenerateText(
                    extraTags: new List<RichText.Tag> { transparentTag },
                    tagMap: tag =>
                            tag == transparentTag
                            ? tag
                            : tag.StartText.StartsWith("<color")
                            ? new RichText.Tag(
                                tag.StartText, tag.EndText,
                                tag.StartIndex, Math.Clamp(charactersVisible, tag.StartIndex, tag.EndIndex)
                            )
                            : tag
                );

                yield return null;
            }

            // We either finished displaying everything, or were
            // interrupted. Either way, display everything now.
            label.text = richText.Substring(0, charactersVisible).Text;

            stopToken?.Complete();
        }


    }
}
