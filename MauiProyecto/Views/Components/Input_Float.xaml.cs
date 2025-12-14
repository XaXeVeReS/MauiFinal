using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.Globalization;

namespace APP_MAUI_Apl_Dis_2025_II.Views.Components;

public partial class Input_Float : Grid
{
    private bool _isInternalChange = false;
    private static readonly decimal MaxValue = 100000m;

    public Input_Float()
    {
        InitializeComponent();
        BindingContext = this;
        UpdateFloatingLabel();
    }

    #region DISENO_GENERAL
    // --- Texto ---
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(Input_Float),
            default(string),
            BindingMode.TwoWay,
            propertyChanged: OnTextChanged);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // --- Placeholder ---
    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(string),
            typeof(Input_Float),
            default(string),
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                var control = (Input_Float)bindable;
                control.floatingLabel.Text = newValue?.ToString();
            });

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    // --- Animación del label ---
    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (Input_Float)bindable;
        control.UpdateFloatingLabel();
    }

    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        AnimateLabelUp();
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        if (string.IsNullOrEmpty(txtInput.Text))
            AnimateLabelDown();
    }

    private void OnCompleted(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txtInput.Text))
            AnimateLabelDown();
    }

    private void UpdateFloatingLabel()
    {
        if (!string.IsNullOrEmpty(txtInput.Text))
            AnimateLabelUp(false);
        else
            AnimateLabelDown(false);
    }

    private void AnimateLabelUp(bool animated = true)
    {
        if (animated)
            floatingLabel.TranslateTo(6, -10, 100, Easing.Linear);
        else
            floatingLabel.TranslationY = -10;

        floatingLabel.FontSize = 12;
        floatingLabel.TextColor = Color.FromArgb("#FF6F00");
    }

    private void AnimateLabelDown(bool animated = true)
    {
        if (animated)
            floatingLabel.TranslateTo(6, 12, 100, Easing.Linear);
        else
            floatingLabel.TranslationY = 12;

        floatingLabel.FontSize = 14;
        floatingLabel.TextColor = Colors.Gray;
    }
    #endregion

    // --- VALIDACIÓN NUMÉRICA DECIMAL ---
    private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isInternalChange) return;

        var newText = e.NewTextValue;

        if (string.IsNullOrWhiteSpace(newText))
            return;

        if (!IsValidFloatFormat(newText))
        {
            UndoChange(e.OldTextValue);
            return;
        }

        if (decimal.TryParse(newText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal value))
        {
            if (DecimalPlaces(newText) > 3)
            {
                UndoChange(e.OldTextValue);
                return;
            }

            if (value >= MaxValue)
            {
                UndoChange(e.OldTextValue);
                return;
            }
        }
        else
        {
            UndoChange(e.OldTextValue);
        }
    }

    private void UndoChange(string oldValue)
    {
        _isInternalChange = true;
        txtInput.Text = oldValue;
        _isInternalChange = false;
    }

    private bool IsValidFloatFormat(string text)
    {
        int dotCount = text.Count(c => c == '.');

        if (dotCount > 1) return false;

        if (text == ".") return false;

        return text.All(c => char.IsDigit(c) || c == '.');
    }

    private int DecimalPlaces(string value)
    {
        int index = value.IndexOf('.');
        if (index < 0) return 0;

        return value.Length - index - 1;
    }
}