using AndroidX.Lifecycle;
using AndroidX.ViewPager2.Adapter;
using EfcToXamarinAndroid.Core;
using Google.Android.Material.Badge;
using System.Collections.Generic;

namespace NavigationDrawerStarter
{
    public partial class MainActivity
    {

        public class CustomViewPager2Adapter : FragmentStateAdapter
        {
            private AndroidX.Fragment.App.FragmentManager _fragmentManager;
            public CustomViewPager2Adapter(AndroidX.Fragment.App.FragmentManager fragmentManager, Lifecycle lifecycle) : base(fragmentManager, lifecycle)
            {
                _fragmentManager = fragmentManager;
            }
            
            public int TabCount { get; set; } = 4;
            public override int ItemCount => TabCount;

            private AndroidX.Fragment.App.Fragment fragment = new AndroidX.Fragment.App.Fragment();


            public override AndroidX.Fragment.App.Fragment CreateFragment(int position)
            {
               
                switch (position)
                {
                    case 0:
                        fragment = new ViewPage2Fragment(position, DatesRepositorio.Payments);
                        break;
                    case 1:
                        fragment = new ViewPage2Fragment(position, DatesRepositorio.Deposits);
                        break;
                    case 2:
                        fragment = new ViewPage2Fragment(position, DatesRepositorio.Cashs);
                        break;
                    case 3:
                        fragment = new ViewPage2Fragment(position, DatesRepositorio.Unreachable);
                        break;
                }
                return fragment;
            }
            
//            public void UpdateFragments()
//            {
//                if (_fragmentManager.Fragments.Count == 0)
//                    return;
//                for (int i = 0; i < _fragmentManager.Fragments.Count; i++)
//                {
//                    var ft = _fragmentManager.Fragments[i];
//                    if (!ft.Tag.Contains("MENU")&& !ft.Tag.Contains("AddItemDialog"))
//                    {
////////                        ((ViewPage2Fragment)ft).DataAdapter.NotifyDataSetChanged();
//                    }
//                }
//            }
        }

    }

}

