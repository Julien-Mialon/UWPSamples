using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using AdaptiveTiles.Model;
using AdaptiveTiles.Services;
using AdaptiveTiles.Tiles;

namespace AdaptiveTiles
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

	    private async void SetTileButtonClicked(object sender, RoutedEventArgs e)
	    {
		    Border border = await TileFactory.SaveLargeTile(new Todo()
		    {
			    Title = "title default",
			    Description = "small description with text wrapping",
			    State = State.Running
		    });

		    Container.Children.Add(border);
	    }
    }
}
