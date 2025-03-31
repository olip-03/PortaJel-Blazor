using CommunityToolkit.Maui.Behaviors;
using Mapsui.UI.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using NetTopologySuite.IO;
using Portajel.Components.Modal;
using Portajel.Connections;
using Portajel.Connections.Interfaces;
using Portajel.Connections.Services.Database;
using Portajel.Connections.Services.Jellyfin;
using System.Drawing;
using Color = Microsoft.Maui.Graphics.Color;

namespace Portajel.Pages.Settings;

public class ConnectionsPage : ContentPage
{
    private Color _primaryDark = Color.FromRgba(0, 0, 0, 255);

    private IServerConnector _server = default!;
    private IDbConnector _database = default!;
    public ConnectionsPage(IServerConnector server, IDbConnector dbConnector)
    {
        if (Application.Current is null) return;
        _server = server;
        _database = dbConnector;

        var imageColor = Application.Current.Resources.TryGetValue("PrimaryDark", out object primaryColor);
        if (imageColor)
        {
            _primaryDark = (Color)primaryColor;
        }

        BuildUI();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        BuildUI();
    }

    private void BuildUI()
    {
        Title = "Connections";

        var mainLayout = new VerticalStackLayout();
        mainLayout.Children.Add(GetConnectionsGrid(_server.Servers.ToArray()));

        mainLayout.Children.Add(new Button
        {
            Margin = 10,
            Text = "Add Connection",
            Command = new Command(async () =>
            {
                JellyfinServerConnector newServer = new JellyfinServerConnector(_database);
                await Navigation.PushModalAsync(new ModalAddServer(_server, newServer) { OnLoginSuccess = ((e) => { BuildUI(); }) }, true);
            })
        });

        Content = mainLayout;
    }

    private Grid GetConnectionsGrid(IMediaServerConnector[] connections)
    {
        var grid = new Grid();
        for (int i = 0; i < connections.Length; i++)
        {
            var serverConnection = connections[i];
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var button = new Button
            {
                Background = new SolidColorBrush(Color.FromRgba(0, 0, 0, 0)),
                Margin = 5,
                ZIndex = 5
            };
            button.Clicked += async (sender, e) => {
                // Pass the entire properties dictionary as a single parameter
                await Shell.Current.GoToAsync("settings/connections/view", new Dictionary<string, object>
                {
                    { "Properties", serverConnection.Properties }
                });
            };

            var view = new VerticalStackLayout
            {
                ZIndex = 10,
                Children =
                {
                    new HorizontalStackLayout
                    {
                        new Image
                        {
                            Margin = 20,
                            Source = serverConnection.Image,
                            HeightRequest = 32,
                            WidthRequest = 32,
                            Behaviors =
                            {
                                new IconTintColorBehavior
                                {
                                    TintColor = _primaryDark
                                }
                            }
                        },
                        new VerticalStackLayout
                        {
                            VerticalOptions = LayoutOptions.Center,
                            Children =
                            {
                                new Label
                                {
                                    Text = serverConnection.Name
                                },
                                new Label
                                {
                                    Text = serverConnection.Description,
                                    FontSize = 14,
                                    HorizontalOptions = LayoutOptions.Start
                                }
                            }
                        }
                    },
                    new ScrollView
                    {
                        CascadeInputTransparent = false,
                        Orientation = ScrollOrientation.Horizontal,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
                        HeightRequest = 40,
                        Margin = new Thickness(10, 0, 10, 0),
                        Content = GetSyncChips(serverConnection)
                    }
                }
            };
            grid.SetRow(button, i);
            grid.SetRow(view, i);

            grid.Children.Add(button);
            grid.Children.Add(view);
        }
        return grid;
    }

    private Grid GetSyncChips(IMediaServerConnector serverConnection)
    {
        Grid mainGrid = new Grid();
        var items = serverConnection.GetDataConnectors();

        for (int i = 0; i < items.Count; i++)
        {
            var dataItem = items.ElementAt(i).Value;
            var dataName = items.ElementAt(i).Key;

            // Create box
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = GridLength.Auto
            });

            // Create Chip
            Border main = new Border
            {
                Stroke = _primaryDark,
                HorizontalOptions = LayoutOptions.Start,
                Padding = new Thickness(4, 2, 8, 2),
                Margin = new Thickness(0, 0, 8, 5),
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = 50
                },
                Content = new HorizontalStackLayout
                {
                    Children =
                    {
                        new Border
                        {
                            HorizontalOptions = LayoutOptions.Start,
                            Background = _primaryDark,
                            Stroke = _primaryDark,
                            Margin = 4,
                            WidthRequest = 24,
                            HeightRequest = 24,
                            StrokeShape = new RoundRectangle
                            {
                                CornerRadius = 12
                            }
                        },
                        new Label
                        {
                            Text = dataName,
                            VerticalOptions = LayoutOptions.Center
                        }
                    }
                }
            };
            mainGrid.SetColumn(main, i);
            mainGrid.Children.Add(main);
        }

        return mainGrid;
    }
}