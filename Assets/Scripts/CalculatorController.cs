using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CalculatorController : MonoBehaviour
{
    [Header("Display References")]
    [SerializeField] private TextMeshProUGUI displayText;

    [Header("Button References")]
    [SerializeField] private Button[] numberButtons;
    [SerializeField] private Button[] operatorButtons;
    [SerializeField] private Button equalsButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button clearLastButton;
    [SerializeField] private Button decimalButton;
    [SerializeField] private Button animationToggleButton;

    [Header("Animation References")]
    [SerializeField] private GameObject clearAnimation;
    [SerializeField] private GameObject numberAnimation;
    [SerializeField] private GameObject operationsAnimation;
    [SerializeField] private GameObject calculateAnimation;

    private string currentExpression = "";
    private bool shouldResetOnNextInput = false;
    private bool animationsEnabled = true;

    private void Start()
    {
        InitializeButtons();
        UpdateDisplay("0");
    }

    private void InitializeButtons()
    {
        if (numberButtons != null)
        {
            for (int i = 0; i < numberButtons.Length; i++)
            {
                int number = i;
                if (numberButtons[i] != null)
                {
                    numberButtons[i].onClick.AddListener(() => OnNumberButtonClicked(number));
                }
            }
        }

        if (operatorButtons != null)
        {
            for (int i = 0; i < operatorButtons.Length; i++)
            {
                int index = i;
                if (operatorButtons[i] != null)
                {
                    operatorButtons[i].onClick.AddListener(() => OnOperatorButtonClicked(index));
                }
            }
        }

        if (equalsButton != null)
            equalsButton.onClick.AddListener(OnEqualsButtonClicked);

        if (clearButton != null)
            clearButton.onClick.AddListener(OnClearButtonClicked);

        if (clearLastButton != null)
            clearLastButton.onClick.AddListener(OnClearLastButtonClicked);

        if (decimalButton != null)
            decimalButton.onClick.AddListener(OnDecimalButtonClicked);

        if (animationToggleButton != null)
            animationToggleButton.onClick.AddListener(OnAnimationToggleClicked);
    }

    private void OnNumberButtonClicked(int number)
    {
        if (shouldResetOnNextInput)
        {
            currentExpression = "";
            shouldResetOnNextInput = false;
        }

        if (number >= 0 && number < numberButtons.Length && numberButtons[number] != null)
        {
            PlayAnimationAtButton(numberAnimation, numberButtons[number]);
        }

        currentExpression += number.ToString();
        UpdateDisplay(currentExpression);
    }

    private void OnOperatorButtonClicked(int operatorIndex)
    {
        if (shouldResetOnNextInput)
        {
            shouldResetOnNextInput = false;
        }

        char[] operators = { '+', '-', '×', '÷' };
        if (operatorIndex >= 0 && operatorIndex < operators.Length && operatorIndex < operatorButtons.Length && operatorButtons[operatorIndex] != null)
        {
            PlayAnimationAtButton(operationsAnimation, operatorButtons[operatorIndex]);
            char op = operators[operatorIndex];

            if (currentExpression.Length > 0 && IsOperator(currentExpression[currentExpression.Length - 1]))
            {
                currentExpression = currentExpression.Substring(0, currentExpression.Length - 1);
            }

            currentExpression += op;
            UpdateDisplay(currentExpression);
        }
    }

    private void OnEqualsButtonClicked()
    {
        if (string.IsNullOrWhiteSpace(currentExpression))
            return;

        PlayAnimationAtButton(calculateAnimation, equalsButton);

        try
        {
            double result = ExpressionEvaluator.Evaluate(currentExpression);
            string resultString = FormatResult(result);
            UpdateDisplay(resultString);
            currentExpression = resultString;
            shouldResetOnNextInput = true;
        }
        catch (System.Exception)
        {
            return;
        }
    }

    private void OnClearButtonClicked()
    {
        PlayAnimationAtButton(clearAnimation, clearButton);
        currentExpression = "";
        shouldResetOnNextInput = false;
        UpdateDisplay("0");
    }

    private void OnClearLastButtonClicked()
    {
        PlayAnimationAtButton(clearAnimation, clearLastButton);

        if (shouldResetOnNextInput)
        {
            currentExpression = "";
            shouldResetOnNextInput = false;
            UpdateDisplay("0");
            return;
        }

        if (string.IsNullOrEmpty(currentExpression))
        {
            UpdateDisplay("0");
            return;
        }

        currentExpression = currentExpression.Substring(0, currentExpression.Length - 1);

        if (string.IsNullOrEmpty(currentExpression))
        {
            UpdateDisplay("0");
        }
        else
        {
            UpdateDisplay(currentExpression);
        }
    }

    private void OnDecimalButtonClicked()
    {
        PlayAnimationAtButton(numberAnimation, decimalButton);

        if (shouldResetOnNextInput)
        {
            currentExpression = "0";
            shouldResetOnNextInput = false;
        }

        int lastOperatorIndex = GetLastOperatorIndex();
        string lastNumber = lastOperatorIndex >= 0 
            ? currentExpression.Substring(lastOperatorIndex + 1) 
            : currentExpression;

        if (!lastNumber.Contains("."))
        {
            if (string.IsNullOrEmpty(currentExpression) || IsOperator(currentExpression[currentExpression.Length - 1]))
            {
                currentExpression += "0";
            }
            currentExpression += ".";
            UpdateDisplay(currentExpression);
        }
    }

    private void UpdateDisplay(string text)
    {
        if (displayText != null)
        {
            displayText.text = text;
        }
    }

    private string FormatResult(double result)
    {
        if (result % 1 == 0)
        {
            return result.ToString("0");
        }
        else
        {
            return result.ToString("G10", System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    private bool IsOperator(char c)
    {
        return c == '+' || c == '-' || c == '×' || c == '÷';
    }

    private int GetLastOperatorIndex()
    {
        for (int i = currentExpression.Length - 1; i >= 0; i--)
        {
            if (IsOperator(currentExpression[i]))
            {
                return i;
            }
        }
        return -1;
    }

    private void OnAnimationToggleClicked()
    {
        animationsEnabled = !animationsEnabled;
        Color color = animationToggleButton.image.color;
        color.a = animationsEnabled ? 1f : 0.5f;
        animationToggleButton.image.color = color;
    }

    private void PlayAnimationAtButton(GameObject animationObject, Button button)
    {
        if (!animationsEnabled || animationObject == null || button == null)
            return;

        RectTransform buttonRect = button.GetComponent<RectTransform>();
        if (buttonRect == null)
            return;

        RectTransform animationRect = animationObject.GetComponent<RectTransform>();
        if (animationRect == null)
            return;

        Vector3 buttonWorldPos = buttonRect.position;
        animationRect.position = buttonWorldPos;

        Animation anim = animationObject.GetComponent<Animation>();
        if (anim != null && anim.clip != null)
        {
            anim.Play();
        }
    }
}
