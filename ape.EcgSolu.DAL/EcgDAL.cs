using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using ape.EcgSolu.IDAL;
using ape.EcgSolu.Model;
using System.Data;
using System.IO;
using System.Diagnostics;

namespace ape.EcgSolu.DAL
{
    public class EcgDAL:IEcgDAL
    {
        public List<Ecg> GetAllList()
        {
            string sqlText = @"select Id,PatientName,Birthday,DataSource,Status,Sex,Age,DiagResult,DiagDoctor,ApplyDoctor,ApplyDept,
                InPatientNo,OutPatientNo,PatientId from ecgs";
            DataTable dbTable = SqlHelper.ExecuteReader(sqlText);
            List<Ecg> ecgList = new List<Ecg>();
            foreach (DataRow dbRow in dbTable.Rows)
            {
                Ecg ecgEntity = new Ecg();
                ecgEntity.Id = new Guid((byte[])dbRow["id"]);
                ecgEntity.PatientName = dbRow["PatientName"] == DBNull.Value ? null : dbRow["PatientName"].ToString();
                if (dbRow["Birthday"] == DBNull.Value)
                {
                    ecgEntity.Birthday = null;
                }
                else
                {
                    ecgEntity.Birthday = DateTime.Parse(dbRow["Birthday"].ToString());
                }                
                ecgEntity.DataSource = dbRow["DataSource"] == DBNull.Value ? null : dbRow["DataSource"].ToString();
                ecgEntity.Status=dbRow["Status"]==DBNull.Value?-1:Convert.ToInt32(dbRow["Status"]);
                ecgEntity.Sex = dbRow["Sex"] == DBNull.Value ? -1 : Convert.ToInt32(dbRow["Sex"]);
                ecgEntity.DiagResult = dbRow["DiagResult"] == DBNull.Value ? null : dbRow["DiagResult"].ToString();
                ecgEntity.DiagResult = dbRow["DiagDoctor"] == DBNull.Value ? null : dbRow["DiagDoctor"].ToString();
                ecgEntity.DiagResult = dbRow["ApplyDoctor"] == DBNull.Value ? null : dbRow["ApplyDoctor"].ToString();
                ecgEntity.DiagResult = dbRow["ApplyDept"] == DBNull.Value ? null : dbRow["ApplyDept"].ToString();
                ecgEntity.DiagResult = dbRow["InPatientNo"] == DBNull.Value ? null : dbRow["InPatientNo"].ToString();
                ecgEntity.DiagResult = dbRow["OutPatientNo"] == DBNull.Value ? null : dbRow["OutPatientNo"].ToString();
                ecgEntity.DiagResult = dbRow["PatientId"] == DBNull.Value ? null : dbRow["PatientId"].ToString();
                ecgList.Add(ecgEntity);
            }
            return ecgList;
        }

        public List<Ecg> GetPagedList(int pageIndex, int pageSize)
        {
            List<Ecg> ecgList = new List<Ecg>();
            int rowsStart = (pageIndex - 1) * pageSize;
            StringBuilder sqlSb = new StringBuilder();
            sqlSb.Append("select Id,PatientName,Birthday,DataSource,Duration,Status,Sex,Age,DiagResult,uVpb,DiagDoctor,");
            sqlSb.Append("ApplyNo,ApplyDept,ApplyDoctor,SamplingDate,SamplingRate,PatientId from ecgs order by rowid desc limit @PageSize offset @RowsStart ");
            SQLiteParameter[] parameters = new SQLiteParameter[]{
                new SQLiteParameter("@RowsStart",rowsStart),
                new SQLiteParameter("@PageSize",pageSize)
            };
            DataTable dbTable = SqlHelper.ExecuteReader(sqlSb.ToString(),parameters);
            foreach (DataRow dbRow in dbTable.Rows)
            {
                Ecg ecgEntity = new Ecg();
                ecgEntity.Id = new Guid((byte[])dbRow["Id"]);
                ecgEntity.PatientName = dbRow["PatientName"] == DBNull.Value ? null : dbRow["PatientName"].ToString();
                if(dbRow["Birthday"] == DBNull.Value)
                {
                    ecgEntity.Birthday =null;
                }
                else
                {
                    ecgEntity.Birthday = Convert.ToDateTime(dbRow["Birthday"]);
                }
                ecgEntity.DataSource = dbRow["DataSource"] == DBNull.Value ? null : dbRow["DataSource"].ToString();
                ecgEntity.Duration = dbRow["Duration"] == DBNull.Value ? 0 : Convert.ToInt32(dbRow["Duration"]);
                ecgEntity.Status = Convert.ToInt32(dbRow["Status"]);
                ecgEntity.Sex = Convert.ToInt32(dbRow["Sex"]);
                ecgEntity.Age = dbRow["Age"] == DBNull.Value ? 0 : Convert.ToInt32(dbRow["Age"]);
                ecgEntity.DiagResult = dbRow["DiagResult"] == DBNull.Value ? null : dbRow["DiagResult"].ToString();
                ecgEntity.uVpb = dbRow["uVpb"] == DBNull.Value ? 0 : Convert.ToSingle(dbRow["uVpb"]);
                if (dbRow["SamplingDate"] == DBNull.Value)
                    ecgEntity.SamplingDate = null;
                else
                    ecgEntity.SamplingDate = Convert.ToDateTime(dbRow["SamplingDate"]);  
                ecgEntity.SamplingRate = dbRow["SamplingRate"] == DBNull.Value ? 0 : Convert.ToInt32(dbRow["SamplingRate"]);
                ecgEntity.PatientId = dbRow["PatientId"] == DBNull.Value ? null : dbRow["PatientId"].ToString();
                ecgList.Add(ecgEntity);
            }
            return ecgList;
        }

        /// <summary>
        /// 获取一条完整记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Ecg GetById(Guid id)
        {
            StringBuilder sqlSb = new StringBuilder();
            sqlSb.Append("select * from ecgs where Id=@Id");
            SQLiteParameter parameter = new SQLiteParameter("@Id", id);
            DataTable dbTable = SqlHelper.ExecuteReader(sqlSb.ToString(), parameter);
            if (dbTable.Rows.Count <= 0)
            {
                throw new Exception("出错方法：EcgDAL.GetById(Guid id)\r\n错误信息：没有找到这条记录,Id:"+id.ToString());
            }
            else if (dbTable.Rows.Count > 1)
            {
                throw new Exception("出错方法：EcgDAL.GetById(Guid id)\r\n错误信息：这条记录不是唯一:"+id.ToString());
            }
            else
            {
                DataRow dbRow = dbTable.Rows[0];
                Ecg ecgEntity = new Ecg();
                ecgEntity.Id = id;
                ecgEntity.PatientName = dbRow["PatientName"] == DBNull.Value ? null : dbRow["PatientName"].ToString();
                if (dbRow["Birthday"] == DBNull.Value)
                {
                    ecgEntity.Birthday = null;
                }
                else
                {
                    ecgEntity.Birthday = Convert.ToDateTime(dbRow["Birthday"]);
                }
                ecgEntity.DataSource = dbRow["DataSource"] == DBNull.Value ? null : dbRow["DataSource"].ToString();
                ecgEntity.Status = Convert.ToInt32(dbRow["Status"]);
                ecgEntity.Sex = Convert.ToInt32(dbRow["Sex"]);
                ecgEntity.Age = dbRow["Age"] == DBNull.Value ? 0 : Convert.ToInt32(dbRow["Age"]);
                ecgEntity.DiagResult = dbRow["DiagResult"] == DBNull.Value ? null : dbRow["DiagResult"].ToString();
                ecgEntity.uVpb = dbRow["uVpb"] == DBNull.Value ? 0 : Convert.ToSingle(dbRow["uVpb"]);
                if (dbRow["SamplingDate"] == DBNull.Value)
                {
                    ecgEntity.SamplingDate = null;
                }
                else
                {
                    ecgEntity.SamplingDate = Convert.ToDateTime(dbRow["SamplingDate"]);
                }               
                ecgEntity.SamplingRate = dbRow["SamplingRate"] == DBNull.Value ? 0 : Convert.ToInt32(dbRow["SamplingRate"]);
                ecgEntity.PatientId = dbRow["PatientId"] == DBNull.Value ? null : dbRow["PatientId"].ToString();
                ecgEntity.Duration = dbRow["Duration"] == DBNull.Value ? 0 : Convert.ToInt32(dbRow["Duration"]);
                ecgEntity.DataCount = dbRow["DataCount"] == DBNull.Value ? 0 : Convert.ToInt32(dbRow["DataCount"]);
                ecgEntity.Data=dbRow["Data"]==DBNull.Value?null:ByteToShort((byte[])dbRow["Data"],12);
                ecgEntity.DataStart = dbRow["DataStart"] == DBNull.Value ? 0 : Convert.ToInt32(dbRow["DataStart"]);
                ecgEntity.QRS = dbRow["QRS"] == DBNull.Value ? 0 : Convert.ToInt32(dbRow["QRS"]);
                ecgEntity.PR = dbRow["PR"] == DBNull.Value ? 0 : Convert.ToInt32(dbRow["PR"]);
                ecgEntity.InsuranceNo = dbRow["InsuranceNo"] == DBNull.Value ? null : dbRow["InsuranceNo"].ToString();
                ecgEntity.ClinicDiag = dbRow["ClinicDiag"] == DBNull.Value ? null : dbRow["ClinicDiag"].ToString();
                ecgEntity.TempData = dbRow["TemplateData"] == DBNull.Value ? null : ByteToShort((byte[])dbRow["TemplateData"], 12);
                ecgEntity.TempDataCount = Convert.ToInt32(dbRow["TempDataCount"]);
                return ecgEntity;
            }
        }

        /// <summary>
        /// 插入一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Insert(Ecg entity)
        {
            StringBuilder sqlSb = new StringBuilder();
            sqlSb.Append("insert into ecgs (ClinicDiag,Id,PatientName,Birthday,DataSource,Sex,Age,DiagResult,uVpb,Duration,DiagDoctor,ApplyNo,");
            sqlSb.Append("ApplyDept,ApplyDate,ApplyDoctor,SamplingDate,SamplingRate,InPatientNo,OutPatientNo,PatientId,PatientTelephone,EcgType,");
            sqlSb.Append("LeadCount,LeadTitle,HighpassFilter,InsuranceNo,LowpassFilter,ACFilter,Data,DataCount,DataStart,TemplateData,TempDataCount,");
            sqlSb.Append("Printed,OnOff,RR,Pd,PR,QRS,Qt,Qtc,Qtd,QtMax,QtMaxLead,QtMin,QtMinLead,AxisP,AxisQRS,AxisT,RV5,RV6,SV1,SV2,RV1,SV5,");
            sqlSb.Append("ExamItem,DiagDept,HeartRate,ModifyDate,IsDelete,DiagDate,TjCpuIdentifier,Status) values");
            sqlSb.Append("(@ClinicDiag,@Id,@PatientName,@Birthday,@DataSource,@Sex,@Age,@DiagResult,@uVpb,@Duration,@DiagDoctor,@ApplyNo,");
            sqlSb.Append("@ApplyDept,@ApplyDate,@ApplyDoctor,@SamplingDate,@SamplingRate,@InPatientNo,@OutPatientNo,@PatientId,@PatientTelephone,@EcgType,");
            sqlSb.Append("@LeadCount,@LeadTitle,@HighpassFilter,@InsuranceNo,@LowpassFilter,@ACFilter,@Data,@DataCount,@DataStart,@TemplateData,@TempDataCount,");
            sqlSb.Append("@Printed,@OnOff,@RR,@Pd,@PR,@QRS,@Qt,@Qtc,@Qtd,@Qtmax,@QtMaxLead,@QtMin,@QtMinLead,@AxisP,@AxisQRS,@AxisT,@RV5,@RV6,@SV1,@SV2,@RV1,@SV5,");
            sqlSb.Append("@ExamItem,@DiagDept,@HeartRate,@ModifyDate,@IsDelete,@DiagDate,@TjCpuIdentifier,@Status)");           
            SQLiteParameter[] parameters = new SQLiteParameter[]{
                new SQLiteParameter("@ClinicDiag",SqlHelper.ToDbValue(entity.ClinicDiag)),
                new SQLiteParameter("@DiagDate",SqlHelper.ToDbValue(entity.DiagDate)),
                new SQLiteParameter("@Id",entity.Id),
                new SQLiteParameter("@PatientName",SqlHelper.ToDbValue(entity.PatientName)),
                new SQLiteParameter("@Birthday",SqlHelper.ToDbValue(entity.Birthday)),            
                new SQLiteParameter("@DataSource",SqlHelper.ToDbValue(entity.DataSource)),
                new SQLiteParameter("@Sex",entity.Sex),
                new SQLiteParameter("@Age",entity.Age),
                new SQLiteParameter("@DiagResult",SqlHelper.ToDbValue(entity.DiagResult)),
                new SQLiteParameter("@uVpb",entity.uVpb),
                new SQLiteParameter("@Duration",entity.Duration),
                new SQLiteParameter("@DiagDoctor",SqlHelper.ToDbValue(entity.DiagDoctor)),             
                new SQLiteParameter("@ApplyNo",SqlHelper.ToDbValue(entity.ApplyNo)),
                new SQLiteParameter("@ApplyDept",SqlHelper.ToDbValue(entity.ApplyDept)),
                new SQLiteParameter("@ApplyDate",SqlHelper.ToDbValue(entity.ApplyDate)),
                new SQLiteParameter("@ApplyDoctor",SqlHelper.ToDbValue(entity.ApplyDoctor)),
                new SQLiteParameter("@SamplingDate",SqlHelper.ToDbValue(entity.SamplingDate)),
                new SQLiteParameter("@SamplingRate",entity.SamplingRate),
                new SQLiteParameter("@InPatientNo",SqlHelper.ToDbValue(entity.InPatientNo)),
                new SQLiteParameter("@OutPatientNo",SqlHelper.ToDbValue(entity.OutPatientNo)),
                new SQLiteParameter("@PatientId",SqlHelper.ToDbValue(entity.PatientId)),
                new SQLiteParameter("@PatientTelephone",SqlHelper.ToDbValue(entity.PatientTelephone)),
                new SQLiteParameter("@EcgType",entity.EcgType),
                new SQLiteParameter("@LeadCount",entity.LeadCount),
                new SQLiteParameter("@LeadTitle",SqlHelper.ToDbValue(entity.LeadTitle)),
                new SQLiteParameter("@HighpassFilter",SqlHelper.ToDbValue(entity.HighpassFilter)),
                new SQLiteParameter("@InsuranceNo",SqlHelper.ToDbValue(entity.InsuranceNo)),
                new SQLiteParameter("@LowpassFilter",SqlHelper.ToDbValue(entity.LowpassFilter)),
                new SQLiteParameter("@ACFilter",SqlHelper.ToDbValue(entity.NotchFilter)),
                new SQLiteParameter("@Data",SqlHelper.ToDbValue(ShortToByte(entity.Data))),
                new SQLiteParameter("@DataCount",entity.DataCount),
                new SQLiteParameter("@DataStart",entity.DataStart),
                new SQLiteParameter("@TemplateData",SqlHelper.ToDbValue(ShortToByte(entity.TempData))),
                new SQLiteParameter("@TempDataCount",entity.TempDataCount),
                new SQLiteParameter("@Printed",entity.Printed),
                new SQLiteParameter("@OnOff",SqlHelper.ToDbValue(entity.OnOff)),
                new SQLiteParameter("@RR",entity.RR),
                new SQLiteParameter("@Pd",entity.Pd),
                new SQLiteParameter("@PR",entity.PR),
                new SQLiteParameter("@QRS",entity.QRS),
                new SQLiteParameter("@Qt",entity.QT),
                new SQLiteParameter("@Qtc",entity.QTc),
                new SQLiteParameter("@Qtd",entity.QTd),
                new SQLiteParameter("@QtMax",entity.QTMax),
                new SQLiteParameter("@QtMaxLead",DBNull.Value),
                new SQLiteParameter("@QtMin",entity.QTMin),
                new SQLiteParameter("@QtMinLead",DBNull.Value),
                new SQLiteParameter("@AxisP",entity.Axis_P),
                new SQLiteParameter("@AxisQRS",entity.Axis_QRS),
                new SQLiteParameter("@AxisT",entity.Axis_T),
                new SQLiteParameter("@RV5",entity.RV5),
                new SQLiteParameter("@RV6",entity.RV6),
                new SQLiteParameter("@SV1",entity.SV1),
                new SQLiteParameter("@SV2",entity.SV2),
                new SQLiteParameter("@RV1",entity.RV1),
                new SQLiteParameter("@SV5",entity.SV5),              
                new SQLiteParameter("@ExamItem",SqlHelper.ToDbValue(entity.ExamItem)),
                new SQLiteParameter("@DiagDept",SqlHelper.ToDbValue(entity.DiagDept)),
                new SQLiteParameter("@HeartRate",entity.HeartRate),
                new SQLiteParameter("@ModifyDate",SqlHelper.ToDbValue(entity.ModifyDate)),
                new SQLiteParameter("@IsDelete",entity.IsDeleted==true?1:0),              
                new SQLiteParameter("@TjCpuIdentifier",SqlHelper.ToDbValue(entity.CpuIdentifier)),
                new SQLiteParameter("@Status",entity.Status),
            };          
            int rows=SqlHelper.ExecuteNoQuery(sqlSb.ToString(), parameters);
            if (rows <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Update(Ecg entity)
        {
            return false;
        }

        /// <summary>
        /// 有选择性部分更新诊断结论相关的内容,保存特征参数和叠加图数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool UpdateResult(Ecg entity)
        {
            StringBuilder sqlSb = new StringBuilder();
            sqlSb.Append("update ecgs set DiagResult=@DiagResult,Status=@Status,TempDataCount=@TemplateDataCount,TemplateData=@TemplateData,");
            sqlSb.Append("HeartRate=@HeartRate,RR=@RR,RV1=@RV1,RV5=@RV5,SV1=@SV1,SV5=@SV5,QRS=@QRS,PR=@PR,DataStart=@DataStart where Id=@Id");
            SQLiteParameter[] parameters = new SQLiteParameter[]{
                new SQLiteParameter("@Id",entity.Id),
                new SQLiteParameter("@DiagResult",entity.DiagResult),
                new SQLiteParameter("@Status",entity.Status),
                new SQLiteParameter("@TemplateDataCount",entity.TempDataCount),
                new SQLiteParameter("@TemplateData",entity.TempData),
                new SQLiteParameter("@HeartRate",entity.HeartRate),
                new SQLiteParameter("@RR",entity.RR),
                new SQLiteParameter("@RV1",entity.RV1),
                new SQLiteParameter("@RV5",entity.RV5),
                new SQLiteParameter("@SV1",entity.SV1),
                new SQLiteParameter("@SV5",entity.SV5),
                new SQLiteParameter("@QRS",entity.QRS),
                new SQLiteParameter("@PR",entity.PR),
                new SQLiteParameter("@DataStart",entity.DataStart),
            };
            int rows = SqlHelper.ExecuteNoQuery(sqlSb.ToString(), parameters);
            if (rows <= 0)
            {
                throw new Exception("EcgDAL.Update(Ecg entity)出错了,rows<=0，Id:" + entity.Id);
            }
            else if (rows > 1)
            {
                throw new Exception("EcgDAL.Update(Ecg entity)出错了,rows>1,Id:" + entity.Id);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteByid(Guid id)
        {
            string sqlText = "delete from ecgs where Id=@Id";
            SQLiteParameter parameter = new SQLiteParameter("@Id", id);
            if (SqlHelper.ExecuteNoQuery(sqlText, parameter) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }           
        }

        /// <summary>
        /// 获取总记录条数
        /// </summary>
        /// <returns></returns>
        public int GetRowsCount()
        {
            string sqlText = "select count(Id) from ecgs";
            object res=SqlHelper.ExecuteScalar(sqlText);
            int rows = Convert.ToInt32(res);
            return rows;
        }

        /// <summary>
        /// 获取符合查询条件的列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="queryArg"></param>
        /// <returns></returns>
        public List<Ecg> GetPagedQueryList(int pageIndex, int pageSize, QueryArg queryArg)
        {
            List<Ecg> ecgList = new List<Ecg>();
            int rowsStart = (pageIndex - 1) * pageSize;
            List<string> whereList = new List<string>();
            List<SQLiteParameter> paramList = new List<SQLiteParameter>();
            this.buildWhereList(queryArg,ref whereList,ref paramList);
            string where = string.Empty;
            if (whereList.Count > 0)
            {
                where += " where ";
                where += string.Join(" and ", whereList);
            }
            StringBuilder sqlSb = new StringBuilder();
            sqlSb.Append("select Id,PatientName,Birthday,DataSource,Duration,Status,Sex,Age,DiagResult,uVpb,DiagDoctor,");
            sqlSb.Append("ApplyNo,ApplyDept,ApplyDoctor,SamplingDate,SamplingRate,PatientId from ecgs ");
            sqlSb.Append(where);
            sqlSb.Append(" order by rowid desc limit @PageSize offset @RowsStart ");            
            SQLiteParameter[] parameters = new SQLiteParameter[]{
                new SQLiteParameter("@RowsStart",rowsStart),
                new SQLiteParameter("@PageSize",pageSize)
            };
            paramList.AddRange(parameters);
            DataTable dbTable = SqlHelper.ExecuteReader(sqlSb.ToString(), paramList.ToArray());
            foreach (DataRow dbRow in dbTable.Rows)
            {
                Ecg ecgEntity = new Ecg();
                ecgEntity.Id = new Guid((byte[])dbRow["Id"]);
                ecgEntity.PatientName = dbRow["PatientName"] == DBNull.Value ? null : dbRow["PatientName"].ToString();
                if (dbRow["Birthday"] == DBNull.Value)
                {
                    ecgEntity.Birthday = null;
                }
                else
                {
                    ecgEntity.Birthday = Convert.ToDateTime(dbRow["Birthday"]);
                }
                ecgEntity.DataSource = dbRow["DataSource"] == DBNull.Value ? null : dbRow["DataSource"].ToString();
                ecgEntity.Duration = dbRow["Duration"] == DBNull.Value ? 0 : Convert.ToInt32(dbRow["Duration"]);
                ecgEntity.Status = Convert.ToInt32(dbRow["Status"]);
                ecgEntity.Sex = Convert.ToInt32(dbRow["Sex"]);
                ecgEntity.Age = dbRow["Age"] == DBNull.Value ? 0 : Convert.ToInt32(dbRow["Age"]);
                ecgEntity.DiagResult = dbRow["DiagResult"] == DBNull.Value ? null : dbRow["DiagResult"].ToString();
                ecgEntity.uVpb = dbRow["uVpb"] == DBNull.Value ? 0 : Convert.ToSingle(dbRow["uVpb"]);
                if (dbRow["SamplingDate"] == DBNull.Value)
                    ecgEntity.SamplingDate = null;
                else
                    ecgEntity.SamplingDate = Convert.ToDateTime(dbRow["SamplingDate"]);                
                ecgEntity.SamplingRate = dbRow["SamplingRate"] == DBNull.Value ? 0 : Convert.ToInt32(dbRow["SamplingRate"]);
                ecgEntity.PatientId = dbRow["PatientId"] == DBNull.Value ? null : dbRow["PatientId"].ToString();
                ecgList.Add(ecgEntity);
            }
            return ecgList;
        }

        /// <summary>
        /// 构建where list和parameter list
        /// </summary>
        /// <param name="queryArg"></param>
        /// <param name="whereList"></param>
        /// <param name="paramList"></param>
        public void buildWhereList(QueryArg queryArg,ref List<string> whereList,ref List<SQLiteParameter> paramList)
        {
            if (!string.IsNullOrWhiteSpace(queryArg.PatientName))
            {
                whereList.Add("PatientName like @PatientName");
                paramList.Add(new SQLiteParameter("@PatientName","%"+queryArg.PatientName+"%"));
            }
            if (queryArg.Gender != null)
            {
                whereList.Add("Sex=@Gender");
                paramList.Add(new SQLiteParameter("@Gender",queryArg.Gender));
            }
            if (!string.IsNullOrWhiteSpace(queryArg.ApplyNo))
            {
                whereList.Add("ApplyNo like @ApplyNo");
                paramList.Add(new SQLiteParameter("@ApplyNo", "%" + queryArg.ApplyNo + "%"));
            }
            if (!string.IsNullOrWhiteSpace(queryArg.InPatientNo))
            {
                whereList.Add("InPatientNo like @InPatientNo");
                paramList.Add(new SQLiteParameter("@InPatientNo", "%" + queryArg.InPatientNo + "%"));
            }
            if (!string.IsNullOrWhiteSpace(queryArg.OutPatientNo))
            {
                whereList.Add("OutPatientNo like @OutPatientNO");
                paramList.Add(new SQLiteParameter("@OutPatientNo", "%" + queryArg.OutPatientNo + "%"));
            }
            if (!string.IsNullOrWhiteSpace(queryArg.ApplyDept))
            {
                whereList.Add("ApplyDept like @ApplyDept");
                paramList.Add(new SQLiteParameter("@ApplyDept", "%" + queryArg.ApplyDept + "%"));
            }
            if (!string.IsNullOrWhiteSpace(queryArg.ApplyDocotor))
            {
                whereList.Add("ApplyDoctor like @ApplyDocotor");
                paramList.Add(new SQLiteParameter("@ApplyDocotor", "%" + queryArg.ApplyDocotor + "%"));
            }
            if (!string.IsNullOrWhiteSpace(queryArg.DiagDocotor))
            {
                whereList.Add("DiagDocotor like @DiagDocotor");
                paramList.Add(new SQLiteParameter("@DiagDocotor", "%" + queryArg.DiagDocotor + "%"));
            }
            if (!string.IsNullOrWhiteSpace(queryArg.DiagResult))
            {
                whereList.Add("DiagResult like @DiagResult");
                paramList.Add(new SQLiteParameter("DiagResult", "%" + queryArg.DiagResult + "%"));
            }
            if (queryArg.DateCheck == true && queryArg.DateStart != null && queryArg.DateEnd != null)
            {
                whereList.Add("SamplingDate between @DateStart and @DateEnd");
                paramList.Add(new SQLiteParameter("@DateStart",queryArg.DateStart));
                paramList.Add(new SQLiteParameter("@DateEnd", queryArg.DateEnd));                
            }
        }

        /// <summary>
        /// 二维short数组，转存为一维字节数组
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] ShortToByte(short[,] data)
        {
            if (data == null || data.Length <= 0)
            {
                return null;
            }
            byte[] buffer;
            using (MemoryStream memStream = new MemoryStream())
            {                
                for (int i = 0; i < data.GetLength(0); i++)
                {
                    for (int j = 0; j < data.GetLength(1); j++)
                    {
                        buffer = BitConverter.GetBytes(data[i, j]);
                        memStream.Write(buffer, 0, 2);
                    }
                }
                buffer = memStream.ToArray();
            }
            return buffer;
        }

        /// <summary>
        /// 一维字节数组，转成二维的short数组
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="leadCount"></param>
        /// <returns></returns>
        private short[,] ByteToShort(byte[] buffer,int leadCount)
        {
            if (leadCount == 0) return null;
            int dataCount = buffer.Length / 2 / leadCount;
            short[,] data = new short[leadCount, dataCount];
            for (int i = 0; i < leadCount; i++)
            {
                for (int j = 0; j < dataCount; j++)
                {
                    data[i, j] = BitConverter.ToInt16(buffer, dataCount * 2 * i + j * 2);
                }
            }
            return data;
        }
    }
}
