using Microsoft.Maui.Controls;

namespace FinanzasFaciles.Controls;

public partial class FlyoutMenuItemView : ContentView
{
	public FlyoutMenuItemView()
	{
		InitializeComponent();
		Loaded += OnLoaded;
		SizeChanged += OnSizeChanged;
	}

		protected override void OnBindingContextChanged()
	{
		base.OnBindingContextChanged();
		ApplyTextBinding();
	}

	private void ApplyTextBinding()
	{
		if (ItemTitle is null) return;
		if (BindingContext is MenuItem m)
		{
			ItemTitle.SetBinding(Label.TextProperty, new Binding(nameof(MenuItem.Text), source: m));
			return;
		}
		if (BindingContext is not null)
			ItemTitle.SetBinding(Label.TextProperty, new Binding("Title", source: BindingContext));
	}

	private void OnLoaded(object? sender, EventArgs e)
	{
		ApplyFlyoutMinWidth();
		ApplyTextBinding();
	}

	private void OnSizeChanged(object? sender, EventArgs e)
	{
		
		if (Width > 1)
			MinimumWidthRequest = Math.Max(MinimumWidthRequest, Width);
	}

	private void ApplyFlyoutMinWidth()
	{
		var w = Shell.Current?.FlyoutWidth ?? 0;
		if (w > 0)
			MinimumWidthRequest = w;
	}
}
