using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using AdaptiveTiles.Model;
using NotificationsExtensions.Tiles;

namespace AdaptiveTiles.Services
{
	class TileService
	{
		public void UpdateMainTile(Todo todo)
		{
			TileBinding binding = new TileBinding
			{
				Branding = TileBranding.None
			};

			TileBindingContentAdaptive tileContent = new TileBindingContentAdaptive();

			TileGroup textGroup = new TileGroup();
			TileSubgroup subgroup = new TileSubgroup
			{
				TextStacking = TileTextStacking.Top
			};

			subgroup.Children.Add(new TileText
			{
				Wrap = true,
				Align = TileTextAlign.Left,
				Style = TileTextStyle.Caption,
				Text = todo.Description
			});
			subgroup.Children.Add(new TileText
			{
				Wrap = false,
				Align = TileTextAlign.Left,
				Style = TileTextStyle.Subtitle,
				Text = todo.Title
			});

			textGroup.Children.Add(subgroup);
			tileContent.Children.Add(textGroup);

			TileGroup botGroup = new TileGroup();
			TileSubgroup leftImageGroup = new TileSubgroup
			{
				TextStacking = TileTextStacking.Bottom,
				Weight = 1
			};
			TileSubgroup rightImageGroup = new TileSubgroup
			{
				TextStacking = TileTextStacking.Bottom,
				Weight = 1
			};

			rightImageGroup.Children.Add(new TileImage
			{
				Align = TileImageAlign.Right,
				Crop = TileImageCrop.Circle,
				Source = new TileImageSource("ms-appx:///Resources/logo.png")
			});
			string state = todo.State == State.Running ? "green" : "red";
			leftImageGroup.Children.Add(new TileImage
			{
				Align = TileImageAlign.Left,
				Crop = TileImageCrop.Circle,
				Source = new TileImageSource($"ms-appx:///Resources/{state}.png")
			});
			botGroup.Children.Add(leftImageGroup);
			//botGroup.Children.Add(rightImageGroup);

			tileContent.Children.Add(botGroup);

			binding.Content = tileContent;

			TileVisual visual = new TileVisual
			{
				TileMedium = binding,
				TileWide = binding,
				TileLarge = binding,
				TileSmall = binding
			};

			TileContent tileObject = new TileContent
			{
				Visual = visual
			};

			Debug.WriteLine(tileObject.GetContent());

			XmlDocument document = new XmlDocument();
			document.LoadXml(tileObject.GetContent());
			UpdateMain(new TileNotification(document));
			//UpdateMain(new TileNotification(tileObject.GetXml()));
		}

		private void UpdateMain(TileNotification notification)
		{
			TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
			updater.Update(notification);
		}

		private void UpdateSecond(TileNotification notification, string id)
		{
			TileUpdater updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(id);
			updater.Update(notification);
		}
	}
}
