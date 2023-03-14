using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadsApp2.Database
{
    public class RoadAccidentDatabase
    {
        SQLiteAsyncConnection Database;

        public RoadAccidentDatabase()
        {

        }

        public async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            await Database.CreateTableAsync<RoadAccidentItem>();
            await Database.CreateTableAsync<ParticipantItem>();
            await Database.CreateTableAsync<RoadAccidentParticipantItem>();
        }


        public async Task<List<RoadAccidentItem>> GetRoadAccidentItemsAsync()
        {
            await Init();
            return await Database.QueryAsync<RoadAccidentItem>("SELECT * FROM [RoadAccidentItem]");
        }

        public async Task<int> SaveRoadAccidentItemAsync(RoadAccidentItem item)
        {
            await Init();
            if (item.ID_RoadAccident != 0)
                return await Database.UpdateAsync(item);
            else
                return await Database.InsertAsync(item);
        }



        //public async Task<TodoItem> GetItemAsync(int id)
        //{
        //    await Init();
        //    await Task.Delay(1000);
        //    return await Database.Table<TodoItem>().Where(i => i.ID == id).FirstOrDefaultAsync();
        //}

        //public async Task<List<TodoItem>> GetItemsAsync()
        //{
        //    await Init();
        //    return await Database.Table<TodoItem>().ToListAsync();
        //}

        //public async Task<int> SaveItemAsync(TodoItem item)
        //{
        //    await Init();
        //    if (item.ID != 0)
        //        return await Database.UpdateAsync(item);
        //    else
        //        return await Database.InsertAsync(item);
        //}

        //public async Task<int> DeleteItemAsync(TodoItem item)
        //{
        //    await Init();
        //    return await Database.DeleteAsync(item);
        //}
    }
}
