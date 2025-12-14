using Microsoft.Maui.Controls;

namespace APP_MAUI_Apl_Dis_2025_II.Views.Components;

public partial class Input_Text : Grid
{
    public Input_Text()
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
            typeof(Input_Text),
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
            typeof(Input_Text),
            default(string),
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                var control = (Input_Text)bindable;
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
        var control = (Input_Text)bindable;
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
}