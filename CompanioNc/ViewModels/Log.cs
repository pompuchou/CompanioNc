﻿using CompanioNc.Models;
using System;
using System.Net;

namespace CompanioNc.ViewModels
{
    public class Logging
    {
        public static void Record_error(string er)
        {
            ///created on 2020/03/28, transcribed from vb.net
            ///寫入錯誤訊息
            using (Com_alDataContext dc = new Com_alDataContext())
            {
                log_Err newErr = new log_Err()
                {
                    error_date = DateTime.Now,
                    application_name = System.Reflection.Assembly.GetExecutingAssembly().FullName.Substring(0, 49),
                    machine_name = Dns.GetHostName(),
                    ip_address = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString(),
                    userid = "Ethan",
                    error_message = er
                };
                dc.log_Err.InsertOnSubmit(newErr);
                dc.SubmitChanges();
            }
        }

        public static void Record_admin(string op, string des)
        {
            ///寫入作業訊息
            using (Com_alDataContext dc = new Com_alDataContext())
            {
                log_Adm newLog = new log_Adm()
                {
                    regdate = DateTime.Now,
                    application_name = System.Reflection.Assembly.GetExecutingAssembly().FullName.Substring(0, 49),
                    machine_name = Dns.GetHostName(),
                    ip_address = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString(),
                    userid = "Ethan",
                    operation_name = op,
                    description = des
                };
                dc.log_Adm.InsertOnSubmit(newLog);
                dc.SubmitChanges();
            }
        }
    }
}