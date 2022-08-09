using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Android.OS;
using Android.Runtime;

using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using AndroidX.ViewPager2.Widget;
using EfcToXamarinAndroid.Core;
using Google.Android.Material.Badge;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Navigation;
using Google.Android.Material.Snackbar;
using Google.Android.Material.Tabs;
using NavigationDrawerStarter.Configs.ManagerCore;
using NavigationDrawerStarter.Fragments;
using NavigationDrawerStarter.Models;
using NavigationDrawerStarter.Parsers;
using Xamarin.Essentials;

namespace NavigationDrawerStarter
{
    [Android.App.Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public partial class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        DrawerLayout drawer;
        RightMenu _RightMenu;
        RightMenuNew _RightMenuNew;
        RightMenuT<DataItem> _RightMenuT;

        private static int[] tabIcons;

        TabLayout tabLayout;
        ViewPager2 pager;
        CustomViewPager2Adapter adapter;

        private static List<BankConfiguration> smsFilters = new List<BankConfiguration>();


        protected override async void OnCreate(Bundle savedInstanceState)
        {
            savedInstanceState = null;
            base.OnCreate(savedInstanceState);
         
            #region Stock
            //base.OnCreate(savedInstanceState);
            if (savedInstanceState == null)
            {
                Xamarin.Essentials.Platform.Init(this, savedInstanceState);
                SetContentView(Resource.Layout.activity_main);

                var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
                SetSupportActionBar(toolbar);

                FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
                fab.Click += FabOnClick;

                drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
                ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
                drawer.AddDrawerListener(toggle);
                //drawer.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
                toggle.SyncState();

                NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
                navigationView.SetNavigationItemSelectedListener(this);
                #endregion

                #region ConfigManager
                ConfigurationManager configManager = ConfigurationManager.ConfigManager;
                var configuration = configManager.BankConfigurationFromJson;
                #endregion

                #region ShowDataFromDb
                await DatesRepositorio.SetDatasFromDB();
                #endregion

                #region Castom Tab

                tabLayout = FindViewById<TabLayout>(Resource.Id.tabLayout);
                tabLayout.InlineLabel = true;
                tabLayout.TabGravity = 0;

                pager = FindViewById<ViewPager2>(Resource.Id.pager);

                tabLayout.TabSelected += (object sender, TabLayout.TabSelectedEventArgs e) =>
                {
                    var tab = e.Tab;
                    var layout = tab.View;

                    var layoutParams = layout.LayoutParameters;// as AndroidX.AppCompat.Widget.LinearLayoutCompat.LayoutParams;

                    tab.SetTabLabelVisibility(TabLayout.TabLabelVisibilityLabeled);

                    layoutParams.Width = LinearLayoutCompat.LayoutParams.WrapContent;

                    layout.LayoutParameters = layoutParams;
                };
                tabLayout.TabUnselected += (object sender, TabLayout.TabUnselectedEventArgs e) =>
                {
                    e.Tab.RemoveBadge();

                    var tab = e.Tab;
                    var layout = tab.View;
                    tab.SetTabLabelVisibility(TabLayout.TabLabelVisibilityUnlabeled);
                    // layoutParams.Width = LinearLayout.LayoutParams.WrapContent;
                };


                adapter = new CustomViewPager2Adapter(this.SupportFragmentManager, this.Lifecycle);
                tabIcons = new int[]
                {
            Resource.Mipmap.ic_cash50,
            Resource.Mipmap.ic_in_deposit50,
            Resource.Mipmap.ic_cashOut51
                };
                pager.Adapter = adapter;

                new TabLayoutMediator(tabLayout, pager, new CustomStrategy()).Attach();
                adapter.NotifyDataSetChanged();

                #endregion

                #region ReadSmS
                //smsFilters.AddRange(configuration.Banks); //This operation took 5420
                //List<Sms> lst = await GetAllSmsAsync(smsFilters);// This operation took 1356
                await ParseSmsToDbAsync(configuration.Banks);//This operation took 56

                #endregion
            }
        }
        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }
        protected override void OnPause()
        {
            base.OnPause();
        }
        protected override void OnStop()
        {
            base.OnStop();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        protected override void OnSaveInstanceState(Bundle outState)
        {
            //outState.PutInt("click_count", _counter);
            //Log.Debug(GetType().FullName, "Activity A - Saving instance state");

            // always call the base implementation!

            var tabSelectedPosition = tabLayout.SelectedTabPosition;
            outState.PutInt("selectedTabPosition", tabSelectedPosition);

            base.OnSaveInstanceState(outState);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
           // base.OnRestoreInstanceState(savedInstanceState);
        }


        private async Task ParseSmsToDbAsync(List<BankConfiguration> bankConfigurations)
        {
            SmsReader smsReader = new SmsReader(this);
            List<Sms> smsList = await smsReader.GetAllSmsAsync(bankConfigurations);

            Parser parserBelarusbank = new Parser(smsList, bankConfigurations);//This operation took 3558
            var data = parserBelarusbank.GetData();
            if (data != null)
            {
                await DatesRepositorio.AddDatas(data);//This operation took 10825
                UpdateBadgeToTabs();
                //adapter.AddNewItemToFragments();
            }
            // adapter.NotifyDataSetChanged();
        }

        private void UpdateBadgeToTabs()
        {
            for (int i = 0; i < tabLayout.TabCount; i++)
            {
                int newItemsCount = 0;
                BadgeDrawable badge;
                switch (i)
                {
                    case 0:
                        newItemsCount = DatesRepositorio.GetPayments(DatesRepositorio.NewDataItems).Count;
                        if (newItemsCount < 1)
                            break;
                        badge = tabLayout.GetTabAt(i).OrCreateBadge;
                        badge.Number = newItemsCount;
                        badge.BadgeGravity = BadgeDrawable.TopStart;
                        break;
                    case 1:
                        newItemsCount = DatesRepositorio.GetDeposits(DatesRepositorio.NewDataItems).Count;
                        if (newItemsCount < 1)
                            break;
                        badge = tabLayout.GetTabAt(i).OrCreateBadge;
                        badge.Number = newItemsCount;
                        badge.BadgeGravity = BadgeDrawable.TopStart;
                        break;
                    case 2:
                        newItemsCount = DatesRepositorio.GetCashs(DatesRepositorio.NewDataItems).Count;
                        if (newItemsCount < 1)
                            break;
                        badge = tabLayout.GetTabAt(i).OrCreateBadge;
                        badge.Number = newItemsCount;
                        badge.BadgeGravity = BadgeDrawable.TopStart;
                        break;
                }
            }
            adapter.UpdateFragments();
        }

        async Task<FileResult> PickAndShow(PickOptions options)
        {
            #region ConfigManager
            ConfigurationManager configManager = ConfigurationManager.ConfigManager;
            var configuration = configManager.BankConfigurationFromJson;
            #endregion
            try
            {
                var result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    Parser parserBelarusbank = new Parser(result.FullPath, configuration.Banks);//This operation took 3558
                    var data = await parserBelarusbank.GetDataFromPdf();
                    await DatesRepositorio.AddDatas(data);//This operation took 10825
                    //adapter.AddNewItemToFragments();
                    //adapter.UpdateFragments();
                    //adapter.NotifyDataSetChanged();
                    UpdateBadgeToTabs();
                }
                return result;
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
            }

            return null;
        }

        #region Stock
        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            //var test = DatesRepositorio.DataItems.Select(x => x.Descripton);
            //var test2 = DatesRepositorio.DataItems.Select(x => x.GetType().GetProperty("Descripton").GetValue(x, null));

            #region FilterBtnClicTest
            //if (id == Resource.Id.action_openRight)
            //{
            //    if (_RightMenu == null)
            //    {
            //        DataItem props = null;

            //        _RightMenuT = new RightMenuT<DataItem>("Descripton", "Date", new string[] { "Id", "HashId", "Balance" }, DatesRepositorio.DataItems);
            //        _RightMenuT.FiltredList = DatesRepositorio.DataItems;


            //        var filterFragmentTransaction = SupportFragmentManager.BeginTransaction();
            //        filterFragmentTransaction.Add(Resource.Id.MenuFragmentFrame, _RightMenuT, "MENU");
            //        filterFragmentTransaction.Commit();
            //        //_RightMenuT.FiltredList = DatesRepositorio.DataItems.Select(x => x.Descripton).ToList<string>();
            //        drawer.OpenDrawer(GravityCompat.End);

            //        //_RightMenu.SetFilters += (object sender, EventArgs e) =>
            //        //{
            //        //    var filter = ((RightMenu)sender).FilredResultList;
            //        //    drawer.CloseDrawer(GravityCompat.End);
            //        //    //drawer.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
            //        //    var fltr = DatesRepositorio.MFilter;
            //        //    fltr.GetResult(x => x.Descripton.Contains("TEST"));
            //        //    adapter.UpdateFragments();
            //        //};
            //        return true;
            //    }
            //    else
            //    {
            //        drawer.OpenDrawer(GravityCompat.End);
            //    }
            //}
            #endregion

            #region FilterBtnClic
            if (id == Resource.Id.action_openRight)
            {
                if (_RightMenu == null)
                {
                    _RightMenu = new RightMenu();

                    var filterFragmentTransaction = SupportFragmentManager.BeginTransaction();
                    filterFragmentTransaction.Add(Resource.Id.MenuFragmentFrame, _RightMenu, "MENU");
                    filterFragmentTransaction.Commit();
                    _RightMenu.FiltredList = DatesRepositorio.DataItems.Select(x => x.Descripton).Distinct().ToList<string>();

                    _RightMenu.AddChekFilterItem("Карта", DatesRepositorio.DataItems.Select(x => x.Karta.ToString()).Distinct().ToList());
                    //_RightMenu.AddChekFilterItem("Категория по умолчанию", DatesRepositorio.DataItems.Select(x => x.DefaultCategoryTyps.ToString()).Distinct().ToList());
                    //_RightMenu.AddChekFilterItem("Пользовательская категория", DatesRepositorio.DataItems.Select(x => x.CastomCategoryTyps.ToString()).Distinct().ToList());

                    _RightMenu.AddChekFilterItem("MCC код", DatesRepositorio.DataItems.Select(x => x.MCC.ToString()).Distinct().ToList());
                    _RightMenu.AddChekFilterItem("MCC описание", DatesRepositorio.DataItems.Select(x => x.MccDeskription?.ToString()).Distinct().ToList());

                    HashSet<string> tags = new HashSet<string>();
                    var subTtags = DatesRepositorio.DataItems.Select(x => x.Title).OfType<String>().Where(x => x != "").Select(x => x.Split(" "));
                    foreach (var tag in subTtags)
                    {
                        tags.UnionWith(tag);
                    }
                    _RightMenu.AddChekFilterItem("Тег", tags.ToList());

                    drawer.OpenDrawer(GravityCompat.End);

                    _RightMenu.SetFilters += (object sender, EventArgs e) =>
                    {

                        var filter = ((RightMenu)sender).FilredResultList;

                        var fltr = DatesRepositorio.MFilter;
                        fltr.GetResult(x => filter.SearchDiscriptions.Length == 0 ? true : filter.SearchDiscriptions.Contains(x.Descripton));
                        fltr.GetResult(x => filter.SearchDatas[0] == default ?
                                                                        x.Date.Date > DateTime.MinValue : (
                                                                        filter.SearchDatas[1] == default ?
                                                                        x.Date.Date == filter.SearchDatas[0] :
                                                                        x.Date.Date >= filter.SearchDatas[0] &&
                                                                        x.Date.Date <= filter.SearchDatas[1]));
                        var chLi = filter.ExpandableListAdapter.childList;
                        foreach (var item in chLi)
                        {
                            if (!item.Key.IsCheked)
                                continue;

                            switch (item.Key.Name)
                            {
                                case "Карта":
                                    fltr.GetResult(q => item.Value.Where(r => r.IsCheked).Select(w => w.Name).Contains(q.Karta.ToString())).ToArray();
                                    break;
                                case "Категория по умолчанию":
                                    fltr.GetResult(q => item.Value.Where(r => r.IsCheked).Select(w => w.Name).Contains(q.DefaultCategoryTyps.ToString())).ToArray();
                                    break;
                                case "Пользовательская категория":
                                    fltr.GetResult(q => item.Value.Where(r => r.IsCheked).Select(w => w.Name).Contains(q.CastomCategoryTyps.ToString())).ToArray();
                                    break;
                                case "MCC код":
                                    fltr.GetResult(q => item.Value.Where(r => r.IsCheked).Select(w => w.Name).Contains(q.MCC.ToString())).ToArray();
                                    break;
                                case "MCC описание":
                                    fltr.GetResult(q => item.Value.Where(r => r.IsCheked).Select(w => w.Name).Contains(q.MccDeskription)).ToArray();
                                    break;
                                case "Тег":
                                    fltr.GetResult(q => item.Value.Where(r => r.IsCheked).Select(w => w.Name).Contains(q.Title)).ToArray();
                                    break;
                            }
                        }
                        drawer.CloseDrawer(GravityCompat.End);
                        adapter.UpdateFragments();
                    };
                    return true;
                }
                else
                {
                    drawer.OpenDrawer(GravityCompat.End);
                }
            }
            #endregion

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            var dialog = new AddItemDialog();
            dialog.AddedItem += (sender, e) =>
            {

                UpdateBadgeToTabs();
                dialog.Dismiss();
            };
            dialog.Display(SupportFragmentManager);
        }


        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.nav_import)
            {
                var options = new PickOptions
                {
                    PickerTitle = "@string/select_pdf_report"
                    //FileTypes = customFileType,
                };

                PickAndShow(options);
            }
            if (id == Resource.Id.nav_db_clear)
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle("Очистка базы данных");
                builder.SetMessage("Данное действие приведет к полной очистке базы данных приложения. " +
                    "Для сохранения возможности востановить данные воспользуйтесь функцией экспорат в файл. " +
                    "Вы действительно ходите продолжить?");

                builder.SetCancelable(false);
                builder.SetPositiveButton("Очистить", async (c, ev) =>
                {
                    string message;
                    message = await DatesRepositorio.DeleteAllItems() ? "База данных очищена." : "Произошла ошибка очистки базы данных.";
                    Android.Widget.Toast.MakeText(this, message, Android.Widget.ToastLength.Short).Show();
                });
                builder.SetNegativeButton("Отмена", (c, ev) =>
                {
                    return;
                });
                builder.Create();
                builder.Show();
                return true;

            }

            if (id == Resource.Id.nav_upload)
            {

                //bool isReadonly = Android.OS.Environment.MediaMountedReadOnly.Equals(Android.OS.Environment.ExternalStorageState);
                //bool isWriteable = Android.OS.Environment.MediaMounted.Equals(Android.OS.Environment.ExternalStorageState);

                //SerializarionToXml serializer = new SerializarionToXml();

                //string documentsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath ;
                //string localFilename = $"FinReport.xml";
                //string localPath = Path.Combine(documentsPath, localFilename);

                //serializer.SaveTofile(localPath);



                //if (_RightMenuNew == null)
                //{
                //    _RightMenuNew = new RightMenuNew();

                //    var filterFragmentTransaction = SupportFragmentManager.BeginTransaction();
                //    filterFragmentTransaction.Add(Resource.Id.MenuFragmentFrame, _RightMenuNew, "MENU");
                //    filterFragmentTransaction.Commit();
                //    _RightMenuNew.FiltredList = DatesRepositorio.DataItems.Select(x => x.Descripton).Distinct().ToList<string>();

                //    _RightMenuNew.AddChekFilterItem("Карта", DatesRepositorio.DataItems.Select(x => x.Karta.ToString()).Distinct().ToList());
                //    //_RightMenu.AddChekFilterItem("Категория по умолчанию", DatesRepositorio.DataItems.Select(x => x.DefaultCategoryTyps.ToString()).Distinct().ToList());
                //    //_RightMenu.AddChekFilterItem("Пользовательская категория", DatesRepositorio.DataItems.Select(x => x.CastomCategoryTyps.ToString()).Distinct().ToList());

                //    _RightMenuNew.AddChekFilterItem("MCC код", DatesRepositorio.DataItems.Select(x => x.MCC.ToString()).Distinct().ToList());
                //    _RightMenuNew.AddChekFilterItem("MCC описание", DatesRepositorio.DataItems.Select(x => x.MccDeskription?.ToString()).Distinct().ToList());


                //    drawer.OpenDrawer(GravityCompat.End);

                //    _RightMenuNew.SetFilters += (object sender, EventArgs e) =>
                //    {

                //        var filter = ((RightMenuNew)sender).FilredResultList;

                //        #region Test
                //        //var filterExtension = DatesRepositorio.DataItems.Where(x => filter.SearchDiscriptions == "" ? true : x.Descripton == filter.SearchDiscriptions)
                //        //                                                .Where(x => filter.SearchDatas[0] == default ?
                //        //                                                x.Date.Date > DateTime.MinValue : (
                //        //                                                filter.SearchDatas[1] == default ?
                //        //                                                x.Date.Date == filter.SearchDatas[0] :
                //        //                                                x.Date.Date > filter.SearchDatas[0] &&
                //        //                                                x.Date.Date < filter.SearchDatas[1])).ToArray();


                //        //var fval = filter.ExpandableListAdapter.childList;
                //        //foreach (var item in fval)
                //        //{
                //        //    if (item.Value.Count == 0)
                //        //        continue;

                //        //    switch (item.Key.Name)
                //        //    {
                //        //        case "Карта":
                //        //            filterExtension = filterExtension.Where(q => item.Value.Where(r => r.IsCheked).Select(w => w.Name).Contains(q.Karta.ToString())).ToArray();
                //        //            break;
                //        //        case "Категория по умолчанию":
                //        //            filterExtension = filterExtension.Where(q => item.Value.Where(r => r.IsCheked).Select(w => w.Name).Contains(q.DefaultCategoryTyps.ToString())).ToArray();
                //        //            break;
                //        //        case "Пользовательская категория":
                //        //            filterExtension = filterExtension.Where(q => item.Value.Where(r => r.IsCheked).Select(w => w.Name).Contains(q.CastomCategoryTyps.ToString())).ToArray();
                //        //            break;


                //        //    }


                //        //}
                //        #endregion

                //        var fltr = DatesRepositorio.MFilter;
                //        fltr.GetResult(x => filter.SearchDiscriptions == "" ? true : x.Descripton == filter.SearchDiscriptions);
                //        fltr.GetResult(x => filter.SearchDatas[0] == default ?
                //                                                        x.Date.Date > DateTime.MinValue : (
                //                                                        filter.SearchDatas[1] == default ?
                //                                                        x.Date.Date == filter.SearchDatas[0] :
                //                                                        x.Date.Date > filter.SearchDatas[0] &&
                //                                                        x.Date.Date < filter.SearchDatas[1]));
                //        var chLi = filter.ExpandableListAdapter.childList;
                //        foreach (var item in chLi)
                //        {
                //            if (!item.Key.IsCheked)
                //                continue;

                //            switch (item.Key.Name)
                //            {
                //                case "Карта":
                //                    fltr.GetResult(q => item.Value.Where(r => r.IsCheked).Select(w => w.Name).Contains(q.Karta.ToString())).ToArray();
                //                    break;
                //                case "Категория по умолчанию":
                //                    fltr.GetResult(q => item.Value.Where(r => r.IsCheked).Select(w => w.Name).Contains(q.DefaultCategoryTyps.ToString())).ToArray();
                //                    break;
                //                case "Пользовательская категория":
                //                    fltr.GetResult(q => item.Value.Where(r => r.IsCheked).Select(w => w.Name).Contains(q.CastomCategoryTyps.ToString())).ToArray();
                //                    break;
                //                case "MCC код":
                //                    fltr.GetResult(q => item.Value.Where(r => r.IsCheked).Select(w => w.Name).Contains(q.MCC.ToString())).ToArray();
                //                    break;
                //                case "MCC описание":
                //                    fltr.GetResult(q => item.Value.Where(r => r.IsCheked).Select(w => w.Name).Contains(q.MccDeskription)).ToArray();
                //                    break;
                //            }
                //        }
                //        drawer.CloseDrawer(GravityCompat.End);
                //        adapter.UpdateFragments();
                //    };
                //    return true;
                //}
                //else
                //{
                //    drawer.OpenDrawer(GravityCompat.End);
                //}
            }

            return true;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        #endregion



    }
}

