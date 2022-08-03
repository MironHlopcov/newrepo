﻿using Android.Graphics.Drawables;
using AndroidX.Core.Content;
using AndroidX.Core.Graphics.Drawable;
using Google.Android.Material.Badge;
using Google.Android.Material.Tabs;
using static Google.Android.Material.Tabs.TabLayoutMediator;

namespace NavigationDrawerStarter
{
    public partial class MainActivity
    {

        public class CustomStrategy : Java.Lang.Object, ITabConfigurationStrategy
        {
            public void OnConfigureTab(TabLayout.Tab p0, int p1)
            {

                Drawable mIcon = ContextCompat.GetDrawable(p0.View.Context, MainActivity.tabIcons[p1]);
                mIcon = DrawableCompat.Wrap(mIcon);
                DrawableCompat.SetTint(mIcon, Android.Graphics.Color.Red);




                //Drawable mIcon = ContextCompat.GetDrawable(p0.View.Context, MainActivity.tabIcons[p1]);
                //mIcon.SetColorFilter(Android.Graphics.Color.Red, Android.Graphics.PorterDuff.Mode.Multiply);


                p0.SetIcon(mIcon);


                //p0.SetText(MainActivity.fragmentTitles[p1]);
              //p0.SetIcon(MainActivity.tabIcons[p1]);
                //p0.SetText("The number of transactions and their amount will be displayed here");
                
                // p0.SetTabLabelVisibility(TabLayout.TabLabelVisibilityUnlabeled);
                p0.SetText(p0.Text);
            }
           
        }

    }

}

