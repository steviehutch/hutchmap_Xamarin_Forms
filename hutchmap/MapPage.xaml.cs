using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

using hutchmap;

namespace hutchmap
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        List<PinData> StevesPins;

        // get todo items...
        TodoItemManager manager;

        Geocoder geoCoder;
        public MapPage()
        {           
            InitializeComponent();
            geoCoder = new Geocoder();

            manager = TodoItemManager.DefaultManager;

            // OnPlatform<T> doesn't currently support the "Windows" target platform, so we have this check here.
            if (manager.IsOfflineEnabled &&
                (Device.OS == TargetPlatform.Windows || Device.OS == TargetPlatform.WinPhone))
            {
                var syncButton = new Button
                {
                    Text = "Sync items",
                    HeightRequest = 30
                };
                syncButton.Clicked += OnSyncItems;

                buttonsPanel.Children.Add(syncButton);
            }

        }

        private void AddMyPin(string name, Position point)
        {
            MyMap.Pins.Add(new Pin
            {
                Label = name,
                Position = point,
                Type = PinType.Generic
            });
        }

        private async void OnAddPinClicked(object sender, EventArgs e)
        {
            var point = MyMap.VisibleRegion.Center;
            var item = (await geoCoder.GetAddressesForPositionAsync(point)).FirstOrDefault();

            var name = item ?? "Unknown";

            AddMyPin(name, point);           
        }

        private void OnStreetClicked(object sender, EventArgs e) =>
            MyMap.MapType = MapType.Street;

        private void OnHybridClicked(object sender, EventArgs e) =>
            MyMap.MapType = MapType.Hybrid;

        private void OnSatelliteClicked(object sender, EventArgs e) =>
            MyMap.MapType = MapType.Satellite;

        private async void OnGoToClicked(object sender, EventArgs e)
        {
            var item = (await geoCoder.GetPositionsForAddressAsync(EntryLocation.Text)).FirstOrDefault();
            if (item == null)
            {
                await DisplayAlert("Error", "Unable to decode position", "OK");
                return;
            }

            var zoomLevel = SliderZoom.Value; // between 1 and 18
            var latlongdegrees = 360 / (Math.Pow(2, zoomLevel));
            MyMap.MoveToRegion(new MapSpan(item, latlongdegrees, latlongdegrees));
        }

        private void OnSliderChanged(object sender, ValueChangedEventArgs e)
        {
            var zoomLevel = e.NewValue; // between 1 and 18
            var latlongdegrees = 360 / (Math.Pow(2, zoomLevel));
            if ((MyMap != null) && (MyMap.VisibleRegion != null))
            {
                MyMap.MoveToRegion(new MapSpan(MyMap.VisibleRegion.Center, latlongdegrees, latlongdegrees));
            }
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Set syncItems to true in order to synchronize the data on startup when running in offline mode
            await RefreshItems(true, syncItems: true);
        }

        // Data methods
        async Task AddItem(TodoItem item)
        {
            await manager.SaveTaskAsync(item);
            todoList.ItemsSource = await manager.GetTodoItemsAsync();
        }

        async Task CompleteItem(TodoItem item)
        {
            if (item.Name == null)
            {
                item.Done = true;
            }
            else
            {
                if (item.Name.StartsWith("safe"))
                {
                    item.Name = item.Name.Replace("safe", "mute");
                }
                else
                {
                    if (item.Name.StartsWith("mute"))
                    {
                        item.Name = item.Name.Replace("mute", "safe");
                    }
                    else
                    {
                        item.Done = true;
                    }
                }
            }
            await manager.SaveTaskAsync(item);
            todoList.ItemsSource = await manager.GetTodoItemsAsync();
        }

        public async void OnAdd(object sender, EventArgs e)
        {
            var todo = new TodoItem { Name = newItemName.Text };
            await AddItem(todo);

            newItemName.Text = string.Empty;
            newItemName.Unfocus();
        }

        // Event handlers
        public async void OnSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var todo = e.SelectedItem as TodoItem;
            if (Device.OS != TargetPlatform.iOS && todo != null)
            {
                // Not iOS - the swipe-to-delete is discoverable there
                if (Device.OS == TargetPlatform.Android)
                {
                    await DisplayAlert(todo.Name, "Press-and-hold to complete task " + todo.Name, "Got it!");
                }
                else
                {
                    // Windows, not all platforms support the Context Actions yet
                    if (await DisplayAlert("Challenge?", "Do you wish to challenge or save this opponent? " + todo.Name + "?", "Challenge/Save", "Cancel"))
                    {                        
                        await CompleteItem(todo);
                    }
                }
            }

            // prevents background getting highlighted
            todoList.SelectedItem = null;
        }

        // http://developer.xamarin.com/guides/cross-platform/xamarin-forms/working-with/listview/#context
        public async void OnComplete(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var todo = mi.CommandParameter as TodoItem;
            await CompleteItem(todo);
        }

        // http://developer.xamarin.com/guides/cross-platform/xamarin-forms/working-with/listview/#pulltorefresh
        public async void OnRefresh(object sender, EventArgs e)
        {
            var list = (ListView)sender;
            Exception error = null;
            try
            {
                await RefreshItems(false, true);
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                list.EndRefresh();
            }

            if (error != null)
            {
                await DisplayAlert("Refresh Error", "Couldn't refresh data (" + error.Message + ")", "OK");
            }
        }

        public async void OnSyncItems(object sender, EventArgs e)
        {
            await RefreshItems(true, true);
        }

        private async Task RefreshItems(bool showActivityIndicator, bool syncItems)
        {
            StevesPins = new List<PinData>();

            using (var scope = new ActivityIndicatorScope(syncIndicator, showActivityIndicator))
            {
                todoList.ItemsSource = await manager.GetTodoItemsAsync(syncItems);                
                StevesPins = await manager.GetPinDataAsync(syncItems);                
            }            

            foreach ( var pin in StevesPins)
            {                
                var pos = new Position(pin.OpLong, pin.OpLat);
                string name = pin.UserName;

                AddMyPin(name, pos);
            }
        }

        private class ActivityIndicatorScope : IDisposable
        {
            private bool showIndicator;
            private ActivityIndicator indicator;
            private Task indicatorDelay;

            public ActivityIndicatorScope(ActivityIndicator indicator, bool showIndicator)
            {
                this.indicator = indicator;
                this.showIndicator = showIndicator;

                if (showIndicator)
                {
                    indicatorDelay = Task.Delay(2000);
                    SetIndicatorActivity(true);
                }
                else
                {
                    indicatorDelay = Task.FromResult(0);
                }
            }

            private void SetIndicatorActivity(bool isActive)
            {
                this.indicator.IsVisible = isActive;
                this.indicator.IsRunning = isActive;
            }

            public void Dispose()
            {
                if (showIndicator)
                {
                    indicatorDelay.ContinueWith(t => SetIndicatorActivity(false), TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }
    }
}
