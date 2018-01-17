using System.Collections;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using UnityEngine;
public class SQLManager : MonoBehaviour
{
    /// <summary>
    /// 数据库连接对象
    /// </summary>
    private SqliteConnection connection;
    /// <summary>
    /// 数据库命令
    /// </summary>
    private SqliteCommand command;
    /// <summary>
    /// 数据读取定义
    /// </summary>
    private SqliteDataReader reader;
    /// <summary>
    /// 本地数据库名字
    /// </summary>
    public string sqlName;

    private static int IDCount;
    public void Start()
    {
        this.CreateSQL();
        this.OpenSQLaAndConnect();
    }
    //创建数据库文件
    public void CreateSQL()
    {
        if (!File.Exists(Application.streamingAssetsPath + "/" + this.sqlName))
        {
            this.connection = new SqliteConnection("data source=" + Application.streamingAssetsPath + "/" + this.sqlName);
            this.connection.Open();
            this.CreateSQLTable(
                "用户",
                "CREATE TABLE 用户(" +
                "ID             INT ," +
                "Name           TEXT," +
                "Age            INT ," +
                "Score          INT," +
                "Time           INT)",
                null,
                null
            );
            this.connection.Close();
            return;
        }
       
    }
    //打开数据库
    public void OpenSQLaAndConnect()
    {
        this.connection = new SqliteConnection("data source=" + Application.streamingAssetsPath + "/" + this.sqlName);
        this.connection.Open();        
    }
    /// <summary>
    ///执行SQL命令,并返回一个SqliteDataReader对象
    /// <param name="queryString"></param>
    public SqliteDataReader ExecuteSQLCommand(string queryString)
    {
        this.command = this.connection.CreateCommand();
        this.command.CommandText = queryString;
        this.reader = this.command.ExecuteReader();
        return this.reader;
    }
    /// <summary>
    /// 通过调用SQL语句，在数据库中创建一个表，顶定义表中的行的名字和对应的数据类型
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnNames"></param>
    /// <param name="dataTypes"></param>
    public SqliteDataReader CreateSQLTable(string tableName, string commandStr=null, string[] columnNames = null, string[] dataTypes = null)
    {
      
        return ExecuteSQLCommand(commandStr);
    }
    /// <summary>
    /// 关闭数据库连接,注意这一步非常重要，最好每次测试结束的时候都调用关闭数据库连接
    /// 如果不执行这一步，多次调用之后，会报错，数据库被锁定，每次打开都非常缓慢
    /// </summary>
    public void CloseSQLConnection()
    {
        if (this.command != null)
        {
            this.command.Cancel();
        }

        if (this.reader != null)
        {
            this.reader.Close();
        }

        if (this.connection != null)
        {
            this.connection.Close();

        }
        this.command = null;
        this.reader = null;
        this.connection = null;
        Debug.Log("已经断开数据库连接");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
           // this.TempInsertData();
            var tempCount = 0;

            ReadTable("用户", new[] { "Age", "Time","Name" }, new[] { "Score" }, new[] { ">=" }, new[] { "10" });
            while (this.reader.Read())
            {
                tempCount++;
                Debug.Log(reader.GetString(reader.GetOrdinal("Name")));
               // Debug.Log(reader.GetInt32(reader.GetOrdinal("Time")));
               // Debug.Log(tempCount);
            }
        }
        if (Input.GetMouseButtonDown(1))
        {          
           //向数据库中插入数据
            InsertDataToSQL("用户", new[] { "1", "'码码小虫'", "22", "100", "100" });
        }
        if (Input.GetMouseButtonDown(2))
        {
            this.GetRowCount();
        }
        // this.CloseSQLConnection();
    }
    /// <summary>
    /// 向数据库中添加数据文件
    /// </summary>
    SqliteDataReader InsertDataToSQL(string tableName,string[] insertValues)
    {
        string commandString= "INSERT INTO " + tableName + " VALUES (" + insertValues[0];
        for (int i = 1; i < insertValues.Length; i++)
        {
            commandString += "," + insertValues[i];
        }
        commandString += ")";
        return ExecuteSQLCommand(commandString);
    }
    /// <summary>
    /// 从数据库中查询相关的数据
    /// </summary>
    void QueryDataFromSQL()
    {

    }

    int GetRowCount()
    {
        this.command = this.connection.CreateCommand();
        this.command.CommandText = "select * from 用户";
        this.reader = this.command.ExecuteReader();        
        DataTable table=new DataTable();
        table.Load(this.reader);
        Debug.Log(table.Rows.Count);
        return table.Rows.Count;
    }

    /// <summary>
    /// 调用数据插入函数，向数据库中插入5条数据
    /// </summary>
    void TempInsertData()
    {
        
        for (int i = 0; i <5; i++)
        {
            InsertDataToSQL(
                "用户",
                new[]
                    {
                        i.ToString(),"'"+"码码小虫"+IDCount.ToString()+"'", (i * 6).ToString(), (i * 50).ToString(),
                        (i*5).ToString()
                    });
            IDCount++;
        }
    }

    /// <summary>
    /// 读取数据表中的数据，返回满足条件的数据
    /// </summary>
    public SqliteDataReader ReadTable(string tableName, string[] items, string[] colNames, string[] operations, string[] colValues)
    {
        string queryString = "SELECT " + items[0];
        for (int i = 1; i < items.Length; i++)
        {
            queryString += ", " + items[i];
        }
        queryString += " FROM " + tableName + " WHERE " + colNames[0] + " " + operations[0] + " " + colValues[0];
        for (int i = 0; i < colNames.Length; i++)
        {
            queryString += " AND " + colNames[i] + " " + operations[i] + " " + colValues[0] + " ";
        }
        return this.ExecuteSQLCommand(queryString);
    }
    private void OnApplicationQuit()
    {
        //当程序退出时关闭数据库连接，不然会重复打开数据卡，造成卡顿
        this.CloseSQLConnection();
        Debug.Log("程序退出");
    }
}
