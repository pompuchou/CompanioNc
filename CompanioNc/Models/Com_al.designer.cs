﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     這段程式碼是由工具產生的。
//     執行階段版本:4.0.30319.42000
//
//     對這個檔案所做的變更可能會造成錯誤的行為，而且如果重新產生程式碼，
//     變更將會遺失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace CompanioNc.Models
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="al")]
	public partial class Com_alDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region 擴充性方法定義
    partial void OnCreated();
    partial void Insertlog_Adm(log_Adm instance);
    partial void Updatelog_Adm(log_Adm instance);
    partial void Deletelog_Adm(log_Adm instance);
    partial void Insertlog_Err(log_Err instance);
    partial void Updatelog_Err(log_Err instance);
    partial void Deletelog_Err(log_Err instance);
    partial void Inserttbl_patients(tbl_patients instance);
    partial void Updatetbl_patients(tbl_patients instance);
    partial void Deletetbl_patients(tbl_patients instance);
    #endregion
		
		public Com_alDataContext() : 
				base(global::CompanioNc.Properties.Settings.Default.alConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public Com_alDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public Com_alDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public Com_alDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public Com_alDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<log_Adm> log_Adm
		{
			get
			{
				return this.GetTable<log_Adm>();
			}
		}
		
		public System.Data.Linq.Table<log_Err> log_Err
		{
			get
			{
				return this.GetTable<log_Err>();
			}
		}
		
		public System.Data.Linq.Table<tbl_patients> tbl_patients
		{
			get
			{
				return this.GetTable<tbl_patients>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.log_Adm")]
	public partial class log_Adm : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _access_id;
		
		private System.DateTime _regdate;
		
		private string _application_name;
		
		private string _machine_name;
		
		private string _ip_address;
		
		private string _userid;
		
		private string _operation_name;
		
		private string _description;
		
    #region 擴充性方法定義
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void Onaccess_idChanging(long value);
    partial void Onaccess_idChanged();
    partial void OnregdateChanging(System.DateTime value);
    partial void OnregdateChanged();
    partial void Onapplication_nameChanging(string value);
    partial void Onapplication_nameChanged();
    partial void Onmachine_nameChanging(string value);
    partial void Onmachine_nameChanged();
    partial void Onip_addressChanging(string value);
    partial void Onip_addressChanged();
    partial void OnuseridChanging(string value);
    partial void OnuseridChanged();
    partial void Onoperation_nameChanging(string value);
    partial void Onoperation_nameChanged();
    partial void OndescriptionChanging(string value);
    partial void OndescriptionChanged();
    #endregion
		
		public log_Adm()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_access_id", AutoSync=AutoSync.OnInsert, DbType="BigInt NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public long access_id
		{
			get
			{
				return this._access_id;
			}
			set
			{
				if ((this._access_id != value))
				{
					this.Onaccess_idChanging(value);
					this.SendPropertyChanging();
					this._access_id = value;
					this.SendPropertyChanged("access_id");
					this.Onaccess_idChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_regdate", DbType="DateTime NOT NULL")]
		public System.DateTime regdate
		{
			get
			{
				return this._regdate;
			}
			set
			{
				if ((this._regdate != value))
				{
					this.OnregdateChanging(value);
					this.SendPropertyChanging();
					this._regdate = value;
					this.SendPropertyChanged("regdate");
					this.OnregdateChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_application_name", DbType="NVarChar(50)")]
		public string application_name
		{
			get
			{
				return this._application_name;
			}
			set
			{
				if ((this._application_name != value))
				{
					this.Onapplication_nameChanging(value);
					this.SendPropertyChanging();
					this._application_name = value;
					this.SendPropertyChanged("application_name");
					this.Onapplication_nameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_machine_name", DbType="NVarChar(50)")]
		public string machine_name
		{
			get
			{
				return this._machine_name;
			}
			set
			{
				if ((this._machine_name != value))
				{
					this.Onmachine_nameChanging(value);
					this.SendPropertyChanging();
					this._machine_name = value;
					this.SendPropertyChanged("machine_name");
					this.Onmachine_nameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ip_address", DbType="NVarChar(50)")]
		public string ip_address
		{
			get
			{
				return this._ip_address;
			}
			set
			{
				if ((this._ip_address != value))
				{
					this.Onip_addressChanging(value);
					this.SendPropertyChanging();
					this._ip_address = value;
					this.SendPropertyChanged("ip_address");
					this.Onip_addressChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_userid", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string userid
		{
			get
			{
				return this._userid;
			}
			set
			{
				if ((this._userid != value))
				{
					this.OnuseridChanging(value);
					this.SendPropertyChanging();
					this._userid = value;
					this.SendPropertyChanged("userid");
					this.OnuseridChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_operation_name", DbType="NVarChar(50)")]
		public string operation_name
		{
			get
			{
				return this._operation_name;
			}
			set
			{
				if ((this._operation_name != value))
				{
					this.Onoperation_nameChanging(value);
					this.SendPropertyChanging();
					this._operation_name = value;
					this.SendPropertyChanged("operation_name");
					this.Onoperation_nameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_description", DbType="Text", UpdateCheck=UpdateCheck.Never)]
		public string description
		{
			get
			{
				return this._description;
			}
			set
			{
				if ((this._description != value))
				{
					this.OndescriptionChanging(value);
					this.SendPropertyChanging();
					this._description = value;
					this.SendPropertyChanged("description");
					this.OndescriptionChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.log_Err")]
	public partial class log_Err : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _error_id;
		
		private System.DateTime _error_date;
		
		private string _application_name;
		
		private string _machine_name;
		
		private string _ip_address;
		
		private string _userid;
		
		private string _error_message;
		
    #region 擴充性方法定義
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void Onerror_idChanging(long value);
    partial void Onerror_idChanged();
    partial void Onerror_dateChanging(System.DateTime value);
    partial void Onerror_dateChanged();
    partial void Onapplication_nameChanging(string value);
    partial void Onapplication_nameChanged();
    partial void Onmachine_nameChanging(string value);
    partial void Onmachine_nameChanged();
    partial void Onip_addressChanging(string value);
    partial void Onip_addressChanged();
    partial void OnuseridChanging(string value);
    partial void OnuseridChanged();
    partial void Onerror_messageChanging(string value);
    partial void Onerror_messageChanged();
    #endregion
		
		public log_Err()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_error_id", AutoSync=AutoSync.OnInsert, DbType="BigInt NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public long error_id
		{
			get
			{
				return this._error_id;
			}
			set
			{
				if ((this._error_id != value))
				{
					this.Onerror_idChanging(value);
					this.SendPropertyChanging();
					this._error_id = value;
					this.SendPropertyChanged("error_id");
					this.Onerror_idChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_error_date", DbType="DateTime NOT NULL")]
		public System.DateTime error_date
		{
			get
			{
				return this._error_date;
			}
			set
			{
				if ((this._error_date != value))
				{
					this.Onerror_dateChanging(value);
					this.SendPropertyChanging();
					this._error_date = value;
					this.SendPropertyChanged("error_date");
					this.Onerror_dateChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_application_name", DbType="NVarChar(50)")]
		public string application_name
		{
			get
			{
				return this._application_name;
			}
			set
			{
				if ((this._application_name != value))
				{
					this.Onapplication_nameChanging(value);
					this.SendPropertyChanging();
					this._application_name = value;
					this.SendPropertyChanged("application_name");
					this.Onapplication_nameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_machine_name", DbType="NVarChar(50)")]
		public string machine_name
		{
			get
			{
				return this._machine_name;
			}
			set
			{
				if ((this._machine_name != value))
				{
					this.Onmachine_nameChanging(value);
					this.SendPropertyChanging();
					this._machine_name = value;
					this.SendPropertyChanged("machine_name");
					this.Onmachine_nameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ip_address", DbType="NVarChar(50)")]
		public string ip_address
		{
			get
			{
				return this._ip_address;
			}
			set
			{
				if ((this._ip_address != value))
				{
					this.Onip_addressChanging(value);
					this.SendPropertyChanging();
					this._ip_address = value;
					this.SendPropertyChanged("ip_address");
					this.Onip_addressChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_userid", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string userid
		{
			get
			{
				return this._userid;
			}
			set
			{
				if ((this._userid != value))
				{
					this.OnuseridChanging(value);
					this.SendPropertyChanging();
					this._userid = value;
					this.SendPropertyChanged("userid");
					this.OnuseridChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_error_message", DbType="Text", UpdateCheck=UpdateCheck.Never)]
		public string error_message
		{
			get
			{
				return this._error_message;
			}
			set
			{
				if ((this._error_message != value))
				{
					this.Onerror_messageChanging(value);
					this.SendPropertyChanging();
					this._error_message = value;
					this.SendPropertyChanged("error_message");
					this.Onerror_messageChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.tbl_patients")]
	public partial class tbl_patients : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private long _cid;
		
		private string _uid;
		
		private string _cname;
		
		private string _mf;
		
		private System.DateTime _bd;
		
		private string _p01;
		
		private string _p02;
		
		private string _p03;
		
		private string _p04;
		
		private System.DateTime _QDATE;
		
    #region 擴充性方法定義
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OncidChanging(long value);
    partial void OncidChanged();
    partial void OnuidChanging(string value);
    partial void OnuidChanged();
    partial void OncnameChanging(string value);
    partial void OncnameChanged();
    partial void OnmfChanging(string value);
    partial void OnmfChanged();
    partial void OnbdChanging(System.DateTime value);
    partial void OnbdChanged();
    partial void Onp01Changing(string value);
    partial void Onp01Changed();
    partial void Onp02Changing(string value);
    partial void Onp02Changed();
    partial void Onp03Changing(string value);
    partial void Onp03Changed();
    partial void Onp04Changing(string value);
    partial void Onp04Changed();
    partial void OnQDATEChanging(System.DateTime value);
    partial void OnQDATEChanged();
    #endregion
		
		public tbl_patients()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_cid", DbType="BigInt NOT NULL")]
		public long cid
		{
			get
			{
				return this._cid;
			}
			set
			{
				if ((this._cid != value))
				{
					this.OncidChanging(value);
					this.SendPropertyChanging();
					this._cid = value;
					this.SendPropertyChanged("cid");
					this.OncidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_uid", DbType="NVarChar(20) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string uid
		{
			get
			{
				return this._uid;
			}
			set
			{
				if ((this._uid != value))
				{
					this.OnuidChanging(value);
					this.SendPropertyChanging();
					this._uid = value;
					this.SendPropertyChanged("uid");
					this.OnuidChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_cname", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string cname
		{
			get
			{
				return this._cname;
			}
			set
			{
				if ((this._cname != value))
				{
					this.OncnameChanging(value);
					this.SendPropertyChanging();
					this._cname = value;
					this.SendPropertyChanged("cname");
					this.OncnameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_mf", DbType="NVarChar(2)")]
		public string mf
		{
			get
			{
				return this._mf;
			}
			set
			{
				if ((this._mf != value))
				{
					this.OnmfChanging(value);
					this.SendPropertyChanging();
					this._mf = value;
					this.SendPropertyChanged("mf");
					this.OnmfChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_bd", DbType="Date NOT NULL")]
		public System.DateTime bd
		{
			get
			{
				return this._bd;
			}
			set
			{
				if ((this._bd != value))
				{
					this.OnbdChanging(value);
					this.SendPropertyChanging();
					this._bd = value;
					this.SendPropertyChanged("bd");
					this.OnbdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_p01", DbType="NVarChar(50)")]
		public string p01
		{
			get
			{
				return this._p01;
			}
			set
			{
				if ((this._p01 != value))
				{
					this.Onp01Changing(value);
					this.SendPropertyChanging();
					this._p01 = value;
					this.SendPropertyChanged("p01");
					this.Onp01Changed();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_p02", DbType="NVarChar(50)")]
		public string p02
		{
			get
			{
				return this._p02;
			}
			set
			{
				if ((this._p02 != value))
				{
					this.Onp02Changing(value);
					this.SendPropertyChanging();
					this._p02 = value;
					this.SendPropertyChanged("p02");
					this.Onp02Changed();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_p03", DbType="NVarChar(50)")]
		public string p03
		{
			get
			{
				return this._p03;
			}
			set
			{
				if ((this._p03 != value))
				{
					this.Onp03Changing(value);
					this.SendPropertyChanging();
					this._p03 = value;
					this.SendPropertyChanged("p03");
					this.Onp03Changed();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_p04", DbType="NVarChar(500)")]
		public string p04
		{
			get
			{
				return this._p04;
			}
			set
			{
				if ((this._p04 != value))
				{
					this.Onp04Changing(value);
					this.SendPropertyChanging();
					this._p04 = value;
					this.SendPropertyChanged("p04");
					this.Onp04Changed();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_QDATE", DbType="DateTime NOT NULL")]
		public System.DateTime QDATE
		{
			get
			{
				return this._QDATE;
			}
			set
			{
				if ((this._QDATE != value))
				{
					this.OnQDATEChanging(value);
					this.SendPropertyChanging();
					this._QDATE = value;
					this.SendPropertyChanged("QDATE");
					this.OnQDATEChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
#pragma warning restore 1591
