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

        public async Task<int> SaveParticipantItemAsync(ParticipantItem item)
        {
            await Init();
            if (item.ID_Participant != 0)
                return await Database.UpdateAsync(item);
            else
                return await Database.InsertAsync(item);
        }

        public async Task<int> SaveRoadAccidentParticipantItemAsync(RoadAccidentParticipantItem item)
        {
            await Init();
            if (item.ID_RoadAccidentParticipant != 0)
                return await Database.UpdateAsync(item);
            else
                return await Database.InsertAsync(item);
        }

        public async Task<RoadAccidentItem> GetRoadAccidentItemAsync(int id)
        {
            await Init();
            return await Database.Table<RoadAccidentItem>().Where(i => i.ID_RoadAccident == id).FirstOrDefaultAsync();
        }

        public async Task<ParticipantItem> GetParticipantItemAsync(int id)
        {
            await Init();
            return await Database.Table<ParticipantItem>().Where(i => i.ID_Participant == id).FirstOrDefaultAsync();
        }

        public async Task<int> GetRoadAccidentItemsAmount()
        {
            await Init();
            return await Database.FindWithQueryAsync<int>("SELECT COUNT() FROM [RoadAccidentItem]");
        }


        //public async Task<int> DeleteItemAsync(TodoItem item)
        //{
        //    await Init();
        //    return await Database.DeleteAsync(item);
        //}
    }
}
