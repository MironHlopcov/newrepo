using EfcToXamarinAndroid.Core;
using Microsoft.EntityFrameworkCore;
using NavigationDrawerStarter.Configs.ManagerCore;
using NavigationDrawerStarter.Filters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NavigationDrawerStarter
{
    public static class DatesRepositorio
    {
        public static List<DataItem> DataItems { get; private set; }
        public static int NewDataItemsCount { get; private set; }
        public static List<DataItem> NewDataItems { get; set; }//will muve  

        public static List<DataItem> Payments = new List<DataItem>();
        public static List<DataItem> Deposits = new List<DataItem>();
        public static List<DataItem> Cashs = new List<DataItem>();
        public static List<DataItem> Unreachable = new List<DataItem>();

        //public static ObservableCollection<DataItem> Payments = new ObservableCollection<DataItem>();
        //public static ObservableCollection<DataItem> Deposits = new ObservableCollection<DataItem>();
        //public static ObservableCollection<DataItem> Cashs = new ObservableCollection<DataItem>();
        //public static ObservableCollection<DataItem> Unreachable = new ObservableCollection<DataItem>();

        private static readonly string dbFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        private static readonly string fileName = "Cats.db";
        private static readonly string dbFullPath = Path.Combine(dbFolder, fileName);

        public static event EventHandler PaymentsChange;
        public static event EventHandler DepositsChange;
        public static event EventHandler CashsChange;
        public static event EventHandler UnreachableChange;

        public static async Task SetDatasFromDB()
        {
            try
            {
                using (var db = new DataItemContext(dbFullPath))
                {
                    await db.Database.MigrateAsync(); //We need to ensure the latest Migration was added. This is different than EnsureDatabaseCreated.
                    DataItems = await db.Cats.ToListAsync();
                    UpdateAutLists(DataItems);
                }
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
        public static async Task AddDatas(List<DataItem> dataItems)
        {
            //var newDataItems = new List<DataItem>();
            var newDataItems = GetNewDatas(dataItems);
            NewDataItems = newDataItems;//will move
            try
            {
                using (var db = new DataItemContext(dbFullPath))
                {
                    await db.Database.MigrateAsync(); //We need to ensure the latest Migration was added. This is different than EnsureDatabaseCreated.
                    if (newDataItems.Count > 0)
                    {
                        await db.Cats.AddRangeAsync(newDataItems);
                        await db.SaveChangesAsync();
                        DataItems.AddRange(newDataItems);
                        UpdateAutLists(DataItems);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
        public static async Task DeleteItem(DataItem dataItem)
        {
            DataItems.Remove(dataItem);
            UpdateAutLists(DataItems);
            try
            {
                using (var db = new DataItemContext(dbFullPath))
                {
                    //await db.Database.MigrateAsync(); //We need to ensure the latest Migration was added. This is different than EnsureDatabaseCreated.
                    db.Entry(dataItem).State = EntityState.Deleted;
                    await db.SaveChangesAsync();

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
        public static async Task<bool> DeleteAllItems()
        {
            try
            {
                using (var db = new DataItemContext(dbFullPath))
                {
                    foreach (var dataItem in DataItems)
                    {
                        db.Cats.Remove(dataItem);
                    }

                    await db.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return false;
            }
        }
        private static List<DataItem> GetNewDatas(List<DataItem> dataItems)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var newDataItems = new List<DataItem>();

            if (DataItems.Count > 0)
            {
                foreach (var item in dataItems)
                {
                    if (item.Date.Second == 0)
                    {
                        if (!DataItems.Any(x => x.HashId == item.HashId))
                            newDataItems.Add(item);
                        else
                        {
                            if (!DataItems.Where(x => x.HashId == item.HashId).Any(x => x.Sum == item.Sum))
                                if (!DataItems.Where(x => x.HashId == item.HashId).Where(x => x.Sum == item.Sum).Any(x => x.OldSum == item.Sum))
                                    newDataItems.Add(item);
                        }
                    }
                    else
                    {
                        if (!DataItems.Any(x => x.Date == item.Date))
                            newDataItems.Add(item);
                        else
                        {
                            if (!DataItems.Where(x => x.Date == item.Date).Any(x => x.Sum == item.Sum))
                                if (!DataItems.Where(x => x.Date == item.Date).Any(x => x.OldSum == item.Sum))
                                    newDataItems.Add(item);
                        }
                    }
                }
            }
            else
                newDataItems = dataItems;
            stopWatch.Stop();
            Console.WriteLine(stopWatch.Elapsed);
            return newDataItems;
        }
        private static void UpdateAutLists(List<DataItem> dataItems)
        {
            List<DataItem> ordetDataItems = dataItems.OrderBy(x => x.Date).Reverse().ToList();
            Payments.Clear();
            Payments.AddRange(GetPayments(ordetDataItems));

            Deposits.Clear();
            Deposits.AddRange(GetDeposits(ordetDataItems));

            Cashs.Clear();
            Cashs.AddRange(GetCashs(ordetDataItems));

            Unreachable.Clear();
            Unreachable.AddRange(GetUnreachable(ordetDataItems));
        }
        public static async Task UpdateItemValue(int id, DataItem newValue)
        {
            var item = DataItems.SingleOrDefault(x => x.Id == id);
            if (item.Sum > newValue.Sum)
            {
                item.Sum = item.Sum - newValue.Sum;
                item.SetNewValues(item);

                while (DatesRepositorio.DataItems.Any(x => x.Date == newValue.Date))
                    newValue.Date = newValue.Date.Second == 59 ?
                        newValue.Date.AddSeconds(-59) :
                        newValue.Date.AddSeconds(1); //создаем потомка с различающимся временем в секундах
                                                     //HashId потомка остается таким же как у родителя
                                                     //при добавлении данных возможно повторное добовление родителя
                newValue.ParentId = item.Id;
                AddDatas(new List<DataItem> { newValue });
            }
            else
                item?.SetNewValues(newValue);
            try
            {
                using (var db = new DataItemContext(dbFullPath))
                {
                    var result = db.Cats.SingleOrDefault(x => x.Id == id);
                    if (result != null)
                    {
                        result.SetNewValues(item);
                        await db.SaveChangesAsync();
                    }
                }

                UpdateAutLists(DataItems);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        public static List<DataItem> GetPayments(List<DataItem> dataItems)
        {
            MccConfigurationManager mccManager = MccConfigurationManager.ConfigManager;
            var codes = mccManager.MccConfigurationFromJson;
            var sdf = dataItems.Where(x => x.OperacionTyp == OperacionTyps.OPLATA).Select(x => x.MccDeskription = codes.Keys.Contains(x.MCC) ? codes[x.MCC] : null).ToList();
            ////return dataItems.Where(x => x.OperacionTyp == OperacionTyps.OPLATA).ToList();
            PaymentsChange?.Invoke(Payments, EventArgs.Empty);
            return dataItems.Where(x => x.OperacionTyp == OperacionTyps.OPLATA).ToList();
        }
        public static List<DataItem> GetDeposits(List<DataItem> dataItems)
        {
            DepositsChange?.Invoke(Deposits, EventArgs.Empty);
            return dataItems.Where(x => x.OperacionTyp == OperacionTyps.ZACHISLENIE).ToList();
        }
        public static List<DataItem> GetCashs(List<DataItem> dataItems)
        {
            CashsChange?.Invoke(Cashs, EventArgs.Empty);
            return dataItems.Where(x => x.OperacionTyp == OperacionTyps.NALICHNYE).ToList();
        }
        public static List<DataItem> GetUnreachable(List<DataItem> dataItems)
        {
            UnreachableChange?.Invoke(Unreachable, EventArgs.Empty);
            return dataItems.Where(x => x.OperacionTyp == OperacionTyps.UNREACHABLE).ToList();
        }

        public static MFilter MFilter
        {
            get
            {
                MFilter mFilter = new MFilter(DataItems);
                mFilter.FiltredClose += MFilter_Filtred;
                return mFilter;
            }
        }
        private static void MFilter_Filtred(object sender)
        {
            UpdateAutLists(((MFilter)sender).OutDataItems);
        }

    }
}