﻿using EfcToXamarinAndroid.Core;
using Microsoft.EntityFrameworkCore;
using NavigationDrawerStarter.Configs.ManagerCore;
using NavigationDrawerStarter.Filters;
using System;
using System.Collections.Generic;
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

        private static readonly string dbFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        private static readonly string fileName = "Cats.db";
        private static readonly string dbFullPath = Path.Combine(dbFolder, fileName);

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
            var newDataItems = new List<DataItem>();

            if (DataItems.Count > 0)
            {
                foreach (var item in dataItems)
                {
                    
                    if (!DataItems.Any(x => x.HashId == item.HashId))
                        newDataItems.Add(item);
                    else
                    {
                        if (!DataItems.Where(x => x.HashId == item.HashId).Any(x => x.Sum == item.Sum))
                            newDataItems.Add(item);
                        else
                        {
                            if (!DataItems.Where(x => x.HashId == item.HashId).Where(x => x.Sum == item.Sum).Any(x => x.Date == item.Date))
                                newDataItems.Add(item);
                        }

                    }

                    //if (!DataItems.Any(x => x.Equals(item)))
                    //    newDataItems.Add(item);
                    //else
                    //{
                    //    if (!dataItems.Where(x => x.Equals(item)).Any(x => x.Sum == item.Sum))
                    //        newDataItems.Add(item);
                    //}

                }
            }
            else
                newDataItems = dataItems;
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

        }
        public static async Task UpdateItemValue(int id, DataItem newValue)
        {
            try
            {
                var item = DataItems.SingleOrDefault(x => x.Id == id);
                item?.SetNewValues(newValue);

                using (var db = new DataItemContext(dbFullPath))
                {
                    var result = db.Cats.SingleOrDefault(x=>x.Id==id);
                    if (result != null)
                    {
                        result.SetNewValues(newValue);
                        await db.SaveChangesAsync();
                    }
                }
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
            return dataItems.Where(x => x.OperacionTyp == OperacionTyps.OPLATA).ToList();
        }
        public static List<DataItem> GetDeposits(List<DataItem> dataItems)
        {
            return dataItems.Where(x => x.OperacionTyp == OperacionTyps.ZACHISLENIE).ToList();
        }
        public static List<DataItem> GetCashs(List<DataItem> dataItems)
        {
            return dataItems.Where(x => x.OperacionTyp == OperacionTyps.NALICHNYE).ToList();
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