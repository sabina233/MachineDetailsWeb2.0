using Microsoft.EntityFrameworkCore;
using web_api2022.Models;


namespace web_api2022.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<DataRecordModel> DataRecords { get; set; }  // 将数据表命名为 DataRecords
        public DbSet<DataModel> DataLatest { get; set; }//新建DataLatest表用于存储最新数据，写入时向两张表中同时写入
        public DbSet<UserModel> Users { get; set; }
        //新增zpl表格
        public DbSet<ZPL_DataRecord> ZPL_DataRecords { get; set; } = null!;
        public DbSet<ZPL_DataLatest> ZPL_DataLatest { get; set; } = null!;
        public DbSet<UserAccount> UserAccounts { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //   modelBuilder.Entity<DataModel>()
        //        .OwnsOne(d => d.Data);  // 指定 Data 作为嵌套对象存储
        //    modelBuilder.Entity<DataRecordModel>()
        //       .OwnsOne(d => d.Data);  // 指定 Data 作为嵌套对象存储
        //    // 为 DataRecords 映射到表 DataRecords
        //    modelBuilder.Entity<DataRecordModel>().ToTable("DataRecords");
        //    // 为 DataLatest 映射到表 DataLatest
        //    modelBuilder.Entity<DataModel>().ToTable("DataLatest");

        //}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataModel>()
                .OwnsOne(d => d.Data, DataDetails =>
                {
                    DataDetails.Property(nd => nd.State).HasColumnName("State");
                    DataDetails.Property(nd => nd.Speed).HasColumnName("Speed");
                    DataDetails.Property(nd => nd.SetSpeed).HasColumnName("SetSpeed");
                    DataDetails.Property(nd => nd.TaskName).HasColumnName("TaskName");
                    DataDetails.Property(nd => nd.TaskNum).HasColumnName("TaskNum");
                    DataDetails.Property(nd => nd.ResidueTask).HasColumnName("ResidueTask");
                    DataDetails.Property(nd => nd.CardReplace).HasColumnName("CardReplace");
                    DataDetails.Property(nd => nd.FileName).HasColumnName("FileName");
                    DataDetails.Property(nd => nd.TotalRecords).HasColumnName("TotalRecords");
                    DataDetails.Property(nd => nd.ProductQuantity).HasColumnName("ProductQuantity");
                    DataDetails.Property(nd => nd.TotalSuccess).HasColumnName("TotalSuccess");
                    DataDetails.Property(nd => nd.TotalFail).HasColumnName("TotalFail");
                    DataDetails.Property(nd => nd.BatchNum).HasColumnName("BatchNum");
                    DataDetails.Property(nd => nd.BatchSuccess).HasColumnName("BatchSuccess");
                    DataDetails.Property(nd => nd.BatchFail).HasColumnName("BatchFail");
                    DataDetails.Property(nd => nd.Trigger1).HasColumnName("Trigger1");
                    DataDetails.Property(nd => nd.Trigger2).HasColumnName("Trigger2");
                    DataDetails.Property(nd => nd.Inventory1).HasColumnName("Inventory1");
                    DataDetails.Property(nd => nd.Inventory2).HasColumnName("Inventory2");
                    DataDetails.Property(nd => nd.Discard1).HasColumnName("Discard1");
                    DataDetails.Property(nd => nd.Discard2).HasColumnName("Discard2");
                    DataDetails.Property(nd => nd.RejectRate).HasColumnName("RejectRate");
                    DataDetails.Property(nd => nd.DataErr).HasColumnName("DataErr");
                });
            modelBuilder.Entity<DataRecordModel>()
                .OwnsOne(d => d.Data, NestedData =>
                {
                    NestedData.Property(nd => nd.State).HasColumnName("State");
                    NestedData.Property(nd => nd.Speed).HasColumnName("Speed");
                    NestedData.Property(nd => nd.SetSpeed).HasColumnName("SetSpeed");
                    NestedData.Property(nd => nd.TaskName).HasColumnName("TaskName");
                    NestedData.Property(nd => nd.TaskNum).HasColumnName("TaskNum");
                    NestedData.Property(nd => nd.ResidueTask).HasColumnName("ResidueTask");
                    NestedData.Property(nd => nd.CardReplace).HasColumnName("CardReplace");
                    NestedData.Property(nd => nd.FileName).HasColumnName("FileName");
                    NestedData.Property(nd => nd.TotalRecords).HasColumnName("TotalRecords");
                    NestedData.Property(nd => nd.ProductQuantity).HasColumnName("ProductQuantity");
                    NestedData.Property(nd => nd.TotalSuccess).HasColumnName("TotalSuccess");
                    NestedData.Property(nd => nd.TotalFail).HasColumnName("TotalFail");
                    NestedData.Property(nd => nd.BatchNum).HasColumnName("BatchNum");
                    NestedData.Property(nd => nd.BatchSuccess).HasColumnName("BatchSuccess");
                    NestedData.Property(nd => nd.BatchFail).HasColumnName("BatchFail");
                    NestedData.Property(nd => nd.Trigger1).HasColumnName("Trigger1");
                    NestedData.Property(nd => nd.Trigger2).HasColumnName("Trigger2");
                    NestedData.Property(nd => nd.Inventory1).HasColumnName("Inventory1");
                    NestedData.Property(nd => nd.Inventory2).HasColumnName("Inventory2");
                    NestedData.Property(nd => nd.Discard1).HasColumnName("Discard1");
                    NestedData.Property(nd => nd.Discard2).HasColumnName("Discard2");
                    NestedData.Property(nd => nd.RejectRate).HasColumnName("RejectRate");
                    NestedData.Property(nd => nd.DataErr).HasColumnName("DataErr");
                });
            // 为 DataRecords 映射到表 DataRecords
            modelBuilder.Entity<DataRecordModel>().ToTable("DataRecords");
            // 为 DataLatest 映射到表 DataLatest
            modelBuilder.Entity<DataModel>().ToTable("DataLatest");
        }
    }
}