using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ketexon.YarnUITK.Tests
{
    [RequireComponent(typeof(UIDocument))]
    public class VisualElementSelectorTest : MonoBehaviour
    {
        [SerializeField] BasicVisualElementSelector _visualElementSelector;

        const string ButtonClass = "test-class-button";
        const string CheckboxClass = "test-class-checkbox";
        const string LabelClass = "test-class-label";

        const string ButtonName = "Button";
        const string CheckboxName = "Checkbox";
        const string LabelName = "Label";

        UIDocument doc;
        VisualElement root;
        UQueryBuilder<VisualElement> queryBuilder;

        void Start()
        {
            doc = GetComponent<UIDocument>();
            root = doc.rootVisualElement;

            var buttonClassSelector = new BasicStringVisualElementSelector($".{ButtonClass}");
            var builder = buttonClassSelector.Builder<VisualElement>(root);
            Debug.Log(builder.First());

            queryBuilder = _visualElementSelector.Builder<VisualElement>(root);
        }

        void Update()
        {
            Debug.Log(queryBuilder.First());
        }
    }
}
