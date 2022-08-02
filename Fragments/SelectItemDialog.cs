using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;

using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using AndroidX.Fragment.App;
using EfcToXamarinAndroid.Core;
using Google.Android.Material.Chip;
using Google.Android.Material.TextField;
using NavigationDrawerStarter.Models;
using NavigationDrawerStarter.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NavigationDrawerStarter.Fragments
{
    public class SelectItemDialog : AndroidX.Fragment.App.DialogFragment, View.IOnClickListener
    {
        public static string TAG = typeof(SelectItemDialog).Name;

        private Toolbar toolbar;
        private Android.Widget.AutoCompleteTextView autocompleteTVOperTyp;
        private TextInputLayout wrap_aut_comp_tv_OperationTyp;
        private TextInputEditText summOper;
        private Android.Widget.AutoCompleteTextView autCompTvOperationDiscription;
        private Android.Widget.AutoCompleteTextView autCompTvOperationMccCode;
        private Android.Widget.AutoCompleteTextView autCompTvOperationMccDiscription;

        private TextInputLayout textfieldDateCheck;
        private TextInputEditText date_text_edit1;

        private TextInputLayout textfieldTimeCheck;
        private TextInputEditText date_text_edit2;

        private TextInputEditText texstInput_CreateChip;
        private ChipGroup chipGroup;

        private List<AddedItemRow> AddedRows = new List<AddedItemRow>();

        private DataItem selectedItem;

        public SelectItemDialog(DataItem dataItem)
        {
            selectedItem = dataItem;
        }

        public void Display(AndroidX.Fragment.App.FragmentManager fragmentManager)
        {
            this.Show(fragmentManager, TAG);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(AndroidX.Fragment.App.DialogFragment.StyleNormal, Resource.Style.AppTheme_FullScreenDialog);
            this.Activity.Window.SetSoftInputMode(SoftInput.AdjustPan | SoftInput.AdjustResize);
        }

        public override void OnStart()
        {
            base.OnStart();
            Dialog dialog = Dialog;
            if (dialog != null)
            {
                int width = ViewGroup.LayoutParams.MatchParent;
                int height = ViewGroup.LayoutParams.MatchParent;
                dialog.Window.SetLayout(width, height);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            View view = inflater.Inflate(Resource.Layout.edititem_dialog, container, false);
            toolbar = (Toolbar)view.FindViewById(Resource.Id.toolbar);

            #region OperTyp
            wrap_aut_comp_tv_OperationTyp = view.FindViewById<TextInputLayout>(Resource.Id.wrap_aut_comp_tv_OperationTyp);
            wrap_aut_comp_tv_OperationTyp.EndIconVisible = false;
            autocompleteTVOperTyp = view.FindViewById<Android.Widget.AutoCompleteTextView>(Resource.Id.aut_comp_tv_OperationTyp);
            autocompleteTVOperTyp.Text = selectedItem.OperacionTyp.ToString();
            autocompleteTVOperTyp.Focusable = false;
            #endregion

            #region Summ
            summOper = view.FindViewById<TextInputEditText>(Resource.Id.texstInput_OperationTSumm);
            summOper.Text = selectedItem.Sum.ToString();
            summOper.Focusable = false;
            #endregion

            #region DateEdit

            textfieldDateCheck = view.FindViewById<TextInputLayout>(Resource.Id.textfield_DateCheck);
            textfieldDateCheck.Tag = "textfieldDateCheck_Tag";

            date_text_edit1 = view.FindViewById<TextInputEditText>(Resource.Id.texstInput_date);
            date_text_edit1.Text = selectedItem.Date.ToShortDateString();
            date_text_edit1.Focusable = false;
            #endregion

            #region TimeEdit

            textfieldTimeCheck = view.FindViewById<TextInputLayout>(Resource.Id.textfield_TimeCheck);
            textfieldTimeCheck.Tag = "textfieldTimeCheck_Tag";

            date_text_edit2 = view.FindViewById<TextInputEditText>(Resource.Id.texstInput_time);
            date_text_edit2.Text = selectedItem.Date.ToLongTimeString();
            date_text_edit2.Focusable = false;

            #endregion

            #region OperationDiscription
            autCompTvOperationDiscription = view.FindViewById<Android.Widget.AutoCompleteTextView>(Resource.Id.aut_comp_tv_OperationDiscription);
            autCompTvOperationDiscription.Text = selectedItem.Descripton;
            autCompTvOperationDiscription.Focusable = false;
            #endregion

            #region OperationMccCode
            autCompTvOperationMccCode = view.FindViewById<Android.Widget.AutoCompleteTextView>(Resource.Id.aut_comp_tv_OperationMccCode);
            autCompTvOperationMccCode.Text = selectedItem.MCC.ToString();
            autCompTvOperationMccCode.Focusable = false;
            #endregion

            #region OperationMccDiscription
            autCompTvOperationMccDiscription = view.FindViewById<Android.Widget.AutoCompleteTextView>(Resource.Id.aut_comp_tv_OperationMccDiscription);
            autCompTvOperationMccDiscription.Text = selectedItem.MccDeskription;
            autCompTvOperationMccDiscription.Focusable = false;
            #endregion

            #region CreateChipInput
            texstInput_CreateChip = view.FindViewById<TextInputEditText>(Resource.Id.texstInput_CreateChip);
            texstInput_CreateChip.SetHeight(0);
            texstInput_CreateChip.Visibility = ViewStates.Invisible;
            #endregion

            #region ChipGroup
            //Распаршевать из строки разделенной пробелмами
            chipGroup = view.FindViewById<ChipGroup>(Resource.Id.chip_group_main);

            HashSet<string> tags = new HashSet<string>();
            var subTtags = DatesRepositorio.DataItems.Select(x => x.Title).OfType<String>().Select(x => x?.Split(" "));
            foreach (var tag in subTtags)
            {
                tags.UnionWith(tag);
            }
            var checkedChips = selectedItem.Title?.Split(" ");
            foreach (string tag in tags)
            {
                bool isChecked = checkedChips?.Contains(tag) ?? false;
                GreateChip(tag, isChecked, inflater);
            }
            #endregion
                
            return view;
        }

        private void TexstInput_CreateChip_EditorAction(object sender, Android.Widget.TextView.EditorActionEventArgs e)
        {
            var tags = texstInput_CreateChip.Text.Trim(' ').Split(" ");
            var inflater = LayoutInflater.From(this.Context);
            var chipsText = new List<string>();
            for (int i = 0; i < chipGroup.ChildCount; i++)
            {
                chipsText.Add(((Chip)chipGroup.GetChildAt(i)).Text);
            }
            foreach (string tag in tags)
            {
                if (chipsText.Any(x => x == tag))
                {
                    Android.Widget.Toast.MakeText(this.Context, $"Тег {tag} уже существует", Android.Widget.ToastLength.Short).Show();
                    continue;
                }
                GreateChip(tag, false, inflater);
            }
            texstInput_CreateChip.Text = "";

            InputMethodManager imm = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(texstInput_CreateChip.WindowToken, 0);
        }
        private void GreateChip(string tag, bool isChecked, LayoutInflater inflater)
        {
            if (tag != "")
            {
                if (isChecked)
                {
                    var chip = (Chip)inflater.Inflate(Resource.Layout.chip_layot, null, false);
                    chip.CloseIconVisible = false;
                    chip.Checkable = false;
                    chip.Text = tag;
                    chip.Checked = true;
                    chipGroup.AddView(chip);
                }
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            toolbar.SetNavigationOnClickListener(this);
           
            toolbar.InflateMenu(Resource.Menu.addItem_dialog);
            toolbar.MenuItemClick += Toolbar_MenuItemClick;
        }
       
        private void Toolbar_MenuItemClick(object sender, Toolbar.MenuItemClickEventArgs e)
        {
            OnEditItemChange(this, e);
        }

        private void Toolbar_NavigationClick(object sender, Toolbar.NavigationClickEventArgs e)
        {
            Dismiss();
        }

        public void OnClick(View v)
        {
            if (v is Chip)
            {
                chipGroup.RemoveView(v);
                return;
            }
            var viewPar = (ViewGroup)v.Parent.Parent.Parent;
            switch (v.Id)
            {
                case Resource.Id.text_input_start_icon:
                    if (viewPar.Tag.ToString() == "textfieldDateCheck_Tag")
                    {
                        new DatePickerFragment(delegate (DateTime datetime)
                        {
                            var _selectedDate = datetime;
                            date_text_edit1.Text = "";
                            date_text_edit1.Text = _selectedDate.ToLongDateString();
                        })
                       .Show(ParentFragmentManager, DatePickerFragment.TAG);
                        textfieldDateCheck.EndIconVisible = true;
                    }
                    if (viewPar.Tag.ToString() == "textfieldTimeCheck_Tag")
                    {
                        new TimePickerFragment(delegate (DateTime datetime)
                        {
                            var _selectedDate = datetime;
                            date_text_edit2.Text = "";
                            date_text_edit2.Text = _selectedDate.ToShortTimeString();
                        })
                        .Show(ParentFragmentManager, TimePickerFragment.TAG);
                        textfieldTimeCheck.EndIconVisible = true;
                    }
                    break;
                case Resource.Id.text_input_end_icon:
                    if (((View)(viewPar.Parent)).Tag.ToString() == "textfieldDateCheck_Tag")
                    {
                        date_text_edit1.Text = "";
                        textfieldDateCheck.EndIconVisible = false;
                    }
                    if (((View)(viewPar.Parent)).Tag.ToString() == "textfieldTimeCheck_Tag")
                    {
                        date_text_edit2.Text = "";
                        textfieldTimeCheck.EndIconVisible = false;
                    }
                    break;
                default:
                    v.Dispose();
                    v = null;
                    var fragment = (AndroidX.Fragment.App.DialogFragment)FragmentManager.FindFragmentByTag(typeof(EditItemDialog).Name);
                    fragment?.Dismiss();
                    break;
            }
        }

        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnDismiss(dialog);
            dialog.Dismiss();
        }

        public delegate void EventHandler(object sender, EventArgs e);
        public event EventHandler EditItemChange;
        protected void OnEditItemChange(object sender, EventArgs e)
        {
            EventHandler handler = EditItemChange;
            handler?.Invoke(this, e);
        }
    }



}