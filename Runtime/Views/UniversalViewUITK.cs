using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Yarn.Unity;


namespace Ketexon.YarnUITK
{
    /// <summary>
    /// A Dialogue View that presents lines of dialogue, using Unity UI
    /// elements.
    /// </summary>
    public class UniversalViewUITK : DialogueViewBase
    {

        [SerializeField]
        internal UIDocument document;
        internal VisualElement root;

        [SerializeField]
        internal BasicVisualElementSelector viewRootSelector;
        internal VisualElement viewRoot = null;

        [SerializeField]
        internal string viewRootEnabledClass = "";
        [SerializeField]
        internal string viewRootPresentedClass = "";
        [SerializeField]
        internal string viewRootDisabledClass = "";

        [SerializeField]
        internal float transitionTime = 0.0f;


        [SerializeField]
        internal BasicVisualElementSelector lineLabelSelector;
        internal Label lineLabel = null;

        internal string lineLabelFullText = null;

        /// <summary>
        /// Controls whether the <see cref="lineText"/> object will show the
        /// character name present in the line or not.
        /// </summary>
        /// <remarks>
        /// <para style="note">This value is only used if <see
        /// cref="characterNameText"/> is <see langword="null"/>.</para>
        /// <para>If this value is <see langword="true"/>, any character names
        /// present in a line will be shown in the <see cref="lineText"/>
        /// object.</para>
        /// <para>If this value is <see langword="false"/>, character names will
        /// not be shown in the <see cref="lineText"/> object.</para>
        /// </remarks>
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("showCharacterName")]
        internal bool showCharacterNameInLineView = true;

        /// <summary>
        /// The <see cref="TextMeshProUGUI"/> object that displays the character
        /// names found in dialogue lines.
        /// </summary>
        /// <remarks>
        /// If the <see cref="LineView"/> receives a line that does not contain
        /// a character name, this object will be left blank.
        /// </remarks>
        [SerializeField]
        internal BasicVisualElementSelector characterNameLabelSelector;
        internal Label characterNameLabel = null;

        [SerializeField]
        internal BasicVisualElementSelector optionContainerSelector;
        internal VisualElement optionContainer = null;

        [SerializeField]
        internal string optionClass = "";

        [SerializeField] bool showUnavailableOptions = false;

        /// <summary>
        /// Controls whether the text of <see cref="lineText"/> should be
        /// gradually revealed over time.
        /// </summary>
        /// <remarks><para>If this value is <see langword="true"/>, the <see
        /// cref="lineText"/> object's <see
        /// cref="TMP_Text.maxVisibleCharacters"/> property will animate from 0
        /// to the length of the text, at a rate of <see
        /// cref="typewriterEffectSpeed"/> letters per second when the line
        /// appears. <see cref="onCharacterTyped"/> is called for every new
        /// character that is revealed.</para>
        /// <para>If this value is <see langword="false"/>, the <see
        /// cref="lineText"/> will all be revealed at the same time.</para>
        /// <para style="note">If <see cref="useFadeEffect"/> is <see
        /// langword="true"/>, the typewriter effect will run after the fade-in
        /// is complete.</para>
        /// </remarks>
        /// <seealso cref="lineText"/>
        /// <seealso cref="onCharacterTyped"/>
        /// <seealso cref="typewriterEffectSpeed"/>
        [SerializeField]
        internal bool useTypewriterEffect = false;

        /// <summary>
        /// A Unity Event that is called each time a character is revealed
        /// during a typewriter effect.
        /// </summary>
        /// <remarks>
        /// This event is only invoked when <see cref="useTypewriterEffect"/> is
        /// <see langword="true"/>.
        /// </remarks>
        /// <seealso cref="useTypewriterEffect"/>
        [SerializeField]
        internal UnityEngine.Events.UnityEvent onCharacterTyped;

        /// <summary>
        /// The number of characters per second that should appear during a
        /// typewriter effect.
        /// </summary>
        /// <seealso cref="useTypewriterEffect"/>
        [SerializeField]
        [Min(0)]
        internal float typewriterEffectSpeed = 0f;

        /// <summary>
        /// The game object that represents an on-screen button that the user
        /// can click to continue to the next piece of dialogue.
        /// </summary>
        /// <remarks>
        /// <para>This game object will be made inactive when a line begins
        /// appearing, and active when the line has finished appearing.</para>
        /// <para>
        /// This field will generally refer to an object that has a <see
        /// cref="Button"/> component on it that, when clicked, calls <see
        /// cref="OnContinueClicked"/>. However, if your game requires specific
        /// UI needs, you can provide any object you need.</para>
        /// </remarks>
        /// <seealso cref="autoAdvance"/>
        [SerializeField]
        internal BasicVisualElementSelector continueButtonSelector;
        internal Button continueButton = null;

        /// <summary>
        /// The amount of time to wait after any line
        /// </summary>
        [SerializeField]
        [Min(0)]
        internal float holdTime = 1f;

        /// <summary>
        /// Controls whether this Line View will wait for user input before
        /// indicating that it has finished presenting a line.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this value is true, the Line View will not report that it has
        /// finished presenting its lines. Instead, it will wait until the <see
        /// cref="UserRequestedViewAdvancement"/> method is called.
        /// </para>
        /// <para style="note"><para>The <see cref="DialogueRunner"/> will not
        /// proceed to the next piece of content (e.g. the next line, or the
        /// next options) until all Dialogue Views have reported that they have
        /// finished presenting their lines. If a <see cref="LineView"/> doesn't
        /// report that it's finished until it receives input, the <see
        /// cref="DialogueRunner"/> will end up pausing.</para>
        /// <para>
        /// This is useful for games in which you want the player to be able to
        /// read lines of dialogue at their own pace, and give them control over
        /// when to advance to the next line.</para></para>
        /// </remarks>
        [SerializeField]
        internal bool autoAdvance = false;

        /// <summary>
        /// The current <see cref="LocalizedLine"/> that this line view is
        /// displaying.
        /// </summary>
        LocalizedLine currentLine = null;

        /// <summary>
        /// A stop token that is used to interrupt the current animation.
        /// </summary>
        Effects.CoroutineInterruptToken currentStopToken = new Effects.CoroutineInterruptToken();

        // A cached pool of OptionView objects so that we can reuse them
        List<Button> optionButtons = new List<Button>();

        Action<int> onOptionSelected;

        LocalizedLine lastSeenLine;

        private void Awake()
        {
            root = document.rootVisualElement;

            viewRoot = viewRootSelector.Builder<VisualElement>(root).First();
            Debug.Assert(viewRoot != null, "viewRootSelector did not find any elements.");
            DisableRoot();

            lineLabel = lineLabelSelector.Builder<Label>(root).First();
            if (lineLabel == null)
            {
                Debug.LogError("lineLabelSelector did not find any elements.");
            }
            characterNameLabel = characterNameLabelSelector.Builder<Label>(root).First();

            optionContainer = optionContainerSelector.Builder<VisualElement>(root).First();

            continueButton = continueButtonSelector.Builder<Button>(root).First();
        }

        void Reset()
        {
            document = GetComponentInParent<UIDocument>();
        }

        /// <inheritdoc/>
        public override void DismissLine(Action onDismissalComplete)
        {
            currentLine = null;

            StartCoroutine(DismissLineInternal(onDismissalComplete));
        }

        private IEnumerator DismissLineInternal(Action onDismissalComplete)
        {
            yield return new WaitForSeconds(0);

            DisableRoot();
            ClearOptions();

            onDismissalComplete();
        }

        /// <inheritdoc/>
        public override void InterruptLine(LocalizedLine dialogueLine, Action onInterruptLineFinished)
        {
            currentLine = dialogueLine;

            // Cancel all coroutines that we're currently running. This will
            // stop the RunLineInternal coroutine, if it's running.
            StopAllCoroutines();

            // for now we are going to just immediately show everything
            // later we will make it fade in
            EnableRoot();

            if (characterNameLabel == null)
            {
                if (showCharacterNameInLineView)
                {
                    lineLabelFullText = dialogueLine.Text.Text;
                }
                else
                {
                    lineLabelFullText = dialogueLine.TextWithoutCharacterName.Text;
                }
            }
            else
            {
                characterNameLabel.text = dialogueLine.CharacterName;
                lineLabelFullText = dialogueLine.TextWithoutCharacterName.Text;
            }

            onInterruptLineFinished();
        }

        /// <inheritdoc/>
        public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            lastSeenLine = dialogueLine;
            // Stop any coroutines currently running on this line view (for
            // example, any other RunLine that might be running)
            StopAllCoroutines();

            // Begin running the line as a coroutine.
            StartCoroutine(RunLineInternal(dialogueLine, onDialogueLineFinished));
        }

        private IEnumerator RunLineInternal(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            ClearOptions();

            IEnumerator PresentLine()
            {
                PresentRoot();

                if (characterNameLabel != null)
                {
                    characterNameLabel.text = dialogueLine.CharacterName;
                    lineLabelFullText = dialogueLine.TextWithoutCharacterName.Text;
                }
                else
                {
                    if (showCharacterNameInLineView)
                    {
                        lineLabelFullText = dialogueLine.Text.Text;
                    }
                    else
                    {
                        lineLabelFullText = dialogueLine.TextWithoutCharacterName.Text;
                    }
                }

                if (useTypewriterEffect)
                {
                    // If we're using the typewriter effect, hide all of the
                    // text before we begin any possible fade (so we don't fade
                    // in on visible text).
                    lineLabel.text = "";
                }
                else
                {
                    // Ensure that the max visible characters is effectively
                    // unlimited.
                    lineLabel.text = lineLabelFullText;
                }

                // If we're using the typewriter effect, start it, and wait for
                // it to finish.
                if (useTypewriterEffect)
                {
                    yield return StartCoroutine(
                        Effects.Typewriter(
                            lineLabelFullText,
                            lineLabel,
                            typewriterEffectSpeed,
                            () => onCharacterTyped.Invoke(),
                            currentStopToken
                        )
                    );
                    if (currentStopToken.WasInterrupted)
                    {
                        // The typewriter effect was interrupted. Stop this
                        // entire coroutine.
                        yield break;
                    }
                }
            }
            currentLine = dialogueLine;

            // Run any presentations as a single coroutine. If this is stopped,
            // which UserRequestedViewAdvancement can do, then we will stop all
            // of the animations at once.
            yield return StartCoroutine(PresentLine());

            currentStopToken.Complete();

            // All of our text should now be visible.
            lineLabel.text = lineLabelFullText;

            // Show the continue button, if we have one.
            if (continueButton != null)
            {
                continueButton.SetEnabled(true);
            }

            // If we have a hold time, wait that amount of time, and then
            // continue.
            if (holdTime > 0)
            {
                yield return new WaitForSeconds(holdTime);
            }

            if (autoAdvance == false)
            {
                // The line is now fully visible, and we've been asked to not
                // auto-advance to the next line. Stop here, and don't call the
                // completion handler - we'll wait for a call to
                // UserRequestedViewAdvancement, which will interrupt this
                // coroutine.
                yield break;
            }

            // Our presentation is complete; call the completion handler.
            onDialogueLineFinished();
        }

        public override void RunOptions(DialogueOption[] dialogueOptions, Action<int> onOptionSelected)
        {
            this.onOptionSelected = onOptionSelected;

            ClearOptions();

            if (lineLabel != null)
            {
                if (showCharacterNameInLineView && characterNameLabel == null)
                {
                    lineLabel.text = lastSeenLine.Text.Text;
                }
                else
                {
                    lineLabel.text = lastSeenLine.TextWithoutCharacterName.Text;
                }
            }
            if (characterNameLabel != null)
            {
                characterNameLabel.text = lastSeenLine.CharacterName ?? "";
            }

            for (int i = 0; i < dialogueOptions.Length; i++)
            {
                var option = dialogueOptions[i];
                if (!option.IsAvailable && !showUnavailableOptions)
                {
                    continue;
                }
                var button = new Button(() => SelectOption(option.DialogueOptionID));
                button.text = dialogueOptions[i].Line.Text.Text;
                button.AddToClassList(optionClass);
                optionContainer.Add(button);
                optionButtons.Add(button);
            }

            PresentRoot();

            optionButtons[0].Focus();
        }

        void SelectOption(int i)
        {
            StartCoroutine(SelectOptionAsync(i));
        }

        IEnumerator SelectOptionAsync(int i)
        {
            yield return new WaitForEndOfFrame();
            DisableRoot();
            onOptionSelected(i);
        }

        void EnableRoot()
        {
            viewRoot.SetEnabled(true);
            viewRoot.RemoveFromClassList(viewRootDisabledClass);
            viewRoot.AddToClassList(viewRootEnabledClass);
        }

        void PresentRoot()
        {
            EnableRoot();
            viewRoot.AddToClassList(viewRootPresentedClass);
        }

        void DisableRoot()
        {
            viewRoot.SetEnabled(false);
            viewRoot.AddToClassList(viewRootDisabledClass);
            viewRoot.RemoveFromClassList(viewRootEnabledClass);
            viewRoot.RemoveFromClassList(viewRootPresentedClass);
        }

        void ClearOptions()
        {
            optionContainer.Clear();
            optionButtons.Clear();
        }

        /// <inheritdoc/>
        public override void UserRequestedViewAdvancement()
        {
            // We received a request to advance the view. If we're in the middle of
            // an animation, skip to the end of it. If we're not current in an
            // animation, interrupt the line so we can skip to the next one.

            // we have no line, so the user just mashed randomly
            if (currentLine == null)
            {
                return;
            }

            // we may want to change this later so the interrupted
            // animation coroutine is what actually interrupts
            // for now this is fine.
            // Is an animation running that we can stop?
            if (currentStopToken.CanInterrupt)
            {
                // Stop the current animation, and skip to the end of whatever
                // started it.
                currentStopToken.Interrupt();
            }
            else
            {
                // No animation is now running. Signal that we want to
                // interrupt the line instead.
                requestInterrupt?.Invoke();
            }
        }

        /// <summary>
        /// Called when the <see cref="continueButton"/> is clicked.
        /// </summary>
        public void OnContinueClicked()
        {
            // When the Continue button is clicked, we'll do the same thing as
            // if we'd received a signal from any other part of the game (for
            // example, if a DialogueAdvanceInput had signalled us.)
            UserRequestedViewAdvancement();
        }
    }

}
