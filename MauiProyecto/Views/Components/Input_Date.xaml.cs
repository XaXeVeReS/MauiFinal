using Microsoft.Maui.Controls;
using System.Globalization;

namespace APP_MAUI_Apl_Dis_2025_II.Views.Components;
public partial class Input_Date : Grid
{
    public Input_Date()
    {
        InitializeComponent();
        BindingContext = this;

        SelectedDate = DateTime.Today;
        UpdateFloatingLabel();
    }

    public static readonly BindableProperty SelectedDateProperty =
        BindableProperty.Create(
            nameof(SelectedDate),
            typeof(DateTime),
            typeof(Input_Date),
            DateTime.Today,
            BindingMode.TwoWay,
            propertyChanged: OnSelectedDateChanged);

    public DateTime SelectedDate
    {
        get => (DateTime)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    private static void OnSelectedDateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (Input_Date)bindable;
        control.UpdateFloatingLabel();
    }

    public event EventHandler<DateTime> SelectedDateChanged;

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(string),
            typeof(Input_Date),
            default(string),
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                var control = (Input_Date)bindable;
                control.floatingLabel.Text = newVal?.ToString();
            });

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    private void OnIconClicked(object sender, EventArgs e)
    {
        hiddenDatePicker.Focus();
    }

    private void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        SelectedDate = e.NewDate;
        UpdateFloatingLabel();

        SelectedDateChanged?.Invoke(this, e.NewDate);
    }


    private void UpdateFloatingLabel()
    {
        if (SelectedDate != DateTime.MinValue)
            AnimateLabelUp(false);
        else
            AnimateLabelDown(false);
    }

    private void AnimateLabelUp(bool animated = true)
    {
        floatingLabel.FontSize = 12;
        floatingLabel.TextColor = Color.FromArgb("#FF6F00");

        if (animated)
            floatingLabel.TranslateTo(6, -10, 100);
        else
            floatingLabel.TranslationY = -10;
    }

    private void AnimateLabelDown(bool animated = true)
    {
        floatingLabel.FontSize = 14;
        floatingLabel.TextColor = Colors.Gray;

        if (animated)
            floatingLabel.TranslateTo(6, 12, 100);
        else
            floatingLabel.TranslationY = 12;
    }
}