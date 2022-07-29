using Android;
using Android.Views;
using Android.Widget;
using EfcToXamarinAndroid.Core;
using NavigationDrawerStarter.Filters;
using System.Collections.Generic;
using System.Globalization;

namespace NavigationDrawerStarter
{
    public class DataAdapter : BaseAdapter<DataItem>
    {
        private readonly AndroidX.Fragment.App.Fragment context;
        private readonly List<DataItem> dataItems;

        public delegate void DataAdapterHandler(AndroidX.Fragment.App.Fragment context);
        public event DataAdapterHandler? OnDataSetChanged;

        public DataAdapter(AndroidX.Fragment.App.Fragment context, List<DataItem> dataItems)
        {
            this.context = context;
            this.dataItems = dataItems;
        }

        public DataAdapter(AndroidX.Fragment.App.Fragment context, int position)
        {
            this.context = context;
            switch (position)
            {
                case 0:
                    this.dataItems = DatesRepositorio.Payments;
                    break;
                case 1:
                    this.dataItems = DatesRepositorio.Deposits;
                    break;
                case 2:
                    this.dataItems = DatesRepositorio.Cashs;
                    break;
            }
        }

        #region Override
        public override DataItem this[int position]
        {
            get
            {
                return dataItems[position];
            }
        }

        public override int Count
        {
            get
            {
                return dataItems.Count;
            }
        }

        public override long GetItemId(int position)
        {
            return dataItems[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? context.LayoutInflater.Inflate(Resource.Layout.list_item, parent, false);// inflate the xml for each item

            var txtSum = view.FindViewById<TextView>(Resource.Id.sum_TextView);
            var txtDeskr = view.FindViewById<TextView>(Resource.Id.deskription_TextView);
            var txtMcc = view.FindViewById<TextView>(Resource.Id.mcc_code_TextView); 
            var txtDate = view.FindViewById<TextView>(Resource.Id.data_TextView);

            txtSum.Text = string.Format("{0:F}", dataItems[position].Sum);
            //txtSum.Text = dataItems[position].Sum.ToString(CultureInfo.InvariantCulture);
            txtDeskr.Text = dataItems[position].Descripton;
            txtDate.Text = dataItems[position].Date.ToShortDateString();
            txtMcc.Text = dataItems[position].MCC == 0 ? "" : $"{dataItems[position].MCC}: {dataItems[position].MccDeskription}";
           
            return view;
        }
        public override void NotifyDataSetChanged()
        {
            base.NotifyDataSetChanged();
            OnDataSetChanged?.Invoke(context);
        }
        #endregion

        public MFilter MFilter
        {
            get
            {
                MFilter mFilter = new MFilter(dataItems);
                return mFilter;
            }
        }
    }
}