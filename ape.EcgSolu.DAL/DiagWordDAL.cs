using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ape.EcgSolu.Model;
using System.Data;
using System.Data.SQLite;

namespace ape.EcgSolu.DAL
{
    public class DiagWordDAL
    {
        public List<DiagWord> GetDiagWord()
        {
            string sqlText = "select * from diagword";
            DataTable dbTable = SqlHelper.ExecuteReader(sqlText);
            List<DiagWord> wordList = new List<DiagWord>();
            foreach (DataRow dbRow in dbTable.Rows)
            {
                DiagWord word = new DiagWord();
                word.Id = new Guid((byte[])dbRow["Id"]);
                word.Word = dbRow["Word"].ToString();
                word.Category = dbRow["Category"].ToString();
                wordList.Add(word);
            }
            return wordList;
        }

        public List<DiagWord> GetWordByCategory(string category)
        {
            string sqlText = "select * from diagword where category=@Category";
            SQLiteParameter parameter = new SQLiteParameter("@Category", category);
            List<DiagWord> wordList = new List<DiagWord>();
            DataTable dbTable = SqlHelper.ExecuteReader(sqlText, parameter);
            foreach (DataRow dbRow in dbTable.Rows)
            {
                DiagWord word = new DiagWord();
                word.Id = new Guid((byte[])dbRow["Id"]);
                word.Word = dbRow["Word"].ToString();
                word.Category = dbRow["Category"].ToString();
                wordList.Add(word);
            }
            return wordList;
        }

        public List<string> GetCategory()
        {
            string sqlText = "select distinct Category from diagword";
            List<string> cateList = new List<string>();
            DataTable dbTable = SqlHelper.ExecuteReader(sqlText);
            foreach (DataRow dbRow in dbTable.Rows)
            {
                cateList.Add(dbRow["Category"].ToString());
            }
            return cateList;
        }

        private void InitWord(Guid id,string word,string category)
        {
            StringBuilder sqlSb = new StringBuilder("insert into diagword");
            sqlSb.Append(" (Id,word,category)");
            sqlSb.Append(" values (@Id,@Word,@Category)");
            SQLiteParameter[] parameters=new SQLiteParameter[]{
                new SQLiteParameter("@Id",id),
                new SQLiteParameter("@Word",word),
                new SQLiteParameter("@Category",category)
            };
            SqlHelper.ExecuteNoQuery(sqlSb.ToString(),parameters);
        }

        public void InitWord()
        {
            InitWord(Guid.NewGuid(),"记录质量欠佳","其他");
            InitWord(Guid.NewGuid(),"肢导电极接反?","其他");
            InitWord(Guid.NewGuid(),"逆钟向转位","其他");
            InitWord(Guid.NewGuid(),"顺钟向转位","其他");
            InitWord(Guid.NewGuid(),"肢导低电压","其他");
            InitWord(Guid.NewGuid(),"胸导低电压","其他");
            InitWord(Guid.NewGuid(),"低电压","其他");
            InitWord(Guid.NewGuid(),"QT间期延长","其他");
            InitWord(Guid.NewGuid(),"QT间期缩短","其他");
            InitWord(Guid.NewGuid(),"右位心(检查电极位置)?","其他");
            InitWord(Guid.NewGuid(),"高大T波","其他");
            InitWord(Guid.NewGuid(),"ST段抬高","其他");
            InitWord(Guid.NewGuid(),"不确定心电轴","心电轴偏转");
            InitWord(Guid.NewGuid(),"轻度心电轴左偏","心电轴偏转");
            InitWord(Guid.NewGuid(),"心电轴右偏","心电轴偏转");
            InitWord(Guid.NewGuid(),"显著心电轴右偏","心电轴偏转");
            InitWord(Guid.NewGuid(),"心电轴左偏","心电轴偏转");
            InitWord(Guid.NewGuid(),"S1，S2，S3图形","心电轴偏转");
            InitWord(Guid.NewGuid(),"左心室高电压","心室肥大和心房负荷");
            InitWord(Guid.NewGuid(),"V1为正T波","心室肥大和心房负荷");
            InitWord(Guid.NewGuid(),"怀疑右心室肥大","心室肥大和心房负荷");
            InitWord(Guid.NewGuid(),"怀疑左心室肥大","心室肥大和心房负荷");
            InitWord(Guid.NewGuid(),"右心室肥大","心室肥大和心房负荷");
            InitWord(Guid.NewGuid(),"左心房扩大","心室肥大和心房负荷");
            InitWord(Guid.NewGuid(),"右心房扩大","心室肥大和心房负荷");
            InitWord(Guid.NewGuid(),"右心室肥大","心室肥大和心房负荷");
            InitWord(Guid.NewGuid(),"双心房扩大","心室肥大和心房负荷");
            InitWord(Guid.NewGuid(),"右心室肥大，右心房扩大","心室肥大和心房负荷");
            InitWord(Guid.NewGuid(),"右心室肥大，左心房扩大","心室肥大和心房负荷");
            InitWord(Guid.NewGuid(),"左心室肥大，左心房扩大","心室肥大和心房负荷");
            InitWord(Guid.NewGuid(),"双心室肥大","心室肥大和心房负荷");
            InitWord(Guid.NewGuid(),"左心室肥大","心室肥大和心房负荷");
            InitWord(Guid.NewGuid(),"左心房负荷过重","心室肥大和心房负荷");
            InitWord(Guid.NewGuid(), "短PR综合症", "房室传导阻滞");
            InitWord(Guid.NewGuid(), "WPW综合症", "房室传导阻滞");
            InitWord(Guid.NewGuid(), "WPW综合症A", "房室传导阻滞");
            InitWord(Guid.NewGuid(), "WPW综合症B", "房室传导阻滞");
            InitWord(Guid.NewGuid(), "I度房室传导阻滞", "房室传导阻滞");
            InitWord(Guid.NewGuid(), "II度房室传导阻滞(文氏)", "房室传导阻滞");
            InitWord(Guid.NewGuid(), "II度房室传导阻滞(莫氏)", "房室传导阻滞");
            InitWord(Guid.NewGuid(), "2:1房室传导阻滞", "房室传导阻滞");
            InitWord(Guid.NewGuid(), "完全性房室传导阻滞", "房室传导阻滞");
            InitWord(Guid.NewGuid(), "起搏器节律(A)", "房室传导阻滞");
            InitWord(Guid.NewGuid(), "起搏器节律(V)", "房室传导阻滞");
            InitWord(Guid.NewGuid(), "起搏器节律(D)", "房室传导阻滞");
            InitWord(Guid.NewGuid(), "感知不良", "房室传导阻滞");
            InitWord(Guid.NewGuid(), "感知过度", "房室传导阻滞");
            InitWord(Guid.NewGuid(), "正常心电图", "诊断结果");
            InitWord(Guid.NewGuid(), "异常心电图", "诊断结果");
        }
    }
}
