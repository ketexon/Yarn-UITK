using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Yarn.Unity;

namespace Ketexon.YarnUITK
{
    public class OptionListViewUITK : DialogueViewBase
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
        internal BasicVisualElementSelector lastCharacterNameLabelSelector;
        internal Label lastCharacterNameLabel = null;

        [SerializeField]
        internal BasicVisualElementSelector lastLineLabelSelector;
        internal Label lastLineLabel = null;

        [SerializeField]
        internal bool showCharacterNameInLineView = true;

        [SerializeField]
        internal BasicVisualElementSelector optionContainerSelector;
        internal VisualElement optionContainer = null;

        [SerializeField]
        internal string optionClass = "";

        [SerializeField] bool showUnavailableOptions = false;

        // A cached pool of OptionView objects so that we can reuse them
        List<Button> optionButtons = new List<Button>();

        // The method we should call when an option has been selected.
        Action<int> onOptionSelected;

        // The line we saw most recently.
        LocalizedLine lastSeenLine;

        void Start()
        {
            root = document.rootVisualElement;

            viewRoot = viewRootSelector.Builder<VisualElement>(root).First();
            Debug.Assert(viewRoot != null, "viewRootSelector did not find any elements.");
            DisableRoot();

            lastCharacterNameLabel = lastCharacterNameLabelSelector.Builder<Label>(root).First();
            lastLineLabel = lastLineLabelSelector.Builder<Label>(root).First();
            optionContainer = optionContainerSelector.Builder<VisualElement>(root).First();
        }

        void Reset()
        {
            document = GetComponentInParent<UIDocument>();
        }

        public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            lastSeenLine = dialogueLine;
            onDialogueLineFinished();
        }

        public override void RunOptions(DialogueOption[] dialogueOptions, Action<int> onOptionSelected)
        {
            this.onOptionSelected = onOptionSelected;

            optionContainer.Clear();
            optionButtons.Clear();

            if (lastLineLabel != null)
            {
                if (showCharacterNameInLineView && lastCharacterNameLabel == null)
                {
                    lastLineLabel.text = lastSeenLine.Text.Text;
                }
                else
                {
                    lastLineLabel.text = lastSeenLine.TextWithoutCharacterName.Text;
                }
            }
            if(lastCharacterNameLabel != null)
            {
                lastCharacterNameLabel.text = lastSeenLine.CharacterName ?? "";
            }

            for(int i = 0; i < dialogueOptions.Length; i++)
            {
                var option = dialogueOptions[i];
                if(!option.IsAvailable && !showUnavailableOptions)
                {
                    continue;
                }
                var button = new Button(() => SelectOption(option.DialogueOptionID));
                button.text = dialogueOptions[i].Line.Text.Text;
                button.AddToClassList(optionClass);
                optionContainer.Add(button);
                optionButtons.Add(button);
            }

            EnableRoot();

            optionButtons[0].Focus();
        }

        void SelectOption(int i)
        {
            StartCoroutine(SelectOptionAsync(i));
        }

        IEnumerator SelectOptionAsync(int i)
        {
            yield return new WaitForEndOfFrame();
            onOptionSelected(i);
            DisableRoot();
        }

        void DisableRoot()
        {
            viewRoot.SetEnabled(false);
            viewRoot.AddToClassList(viewRootDisabledClass);
            viewRoot.RemoveFromClassList(viewRootEnabledClass);
            viewRoot.RemoveFromClassList(viewRootPresentedClass);
        }

        void EnableRoot()
        {
            viewRoot.SetEnabled(true);
            viewRoot.RemoveFromClassList(viewRootDisabledClass);
            viewRoot.AddToClassList(viewRootEnabledClass);
        }

        void PresentRoot()
        {
            viewRoot.AddToClassList(viewRootPresentedClass);
        }
    }
}
