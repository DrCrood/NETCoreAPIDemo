using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using NetCoreAPI.Interfaces;

namespace NetCoreAPI.Models
{
    public class Employee : IEmployee
    {
        //EventHandle is predefined delegate
        public event EventHandler salaryChanged;
        public event EventHandler<EmployeeDataChangedEventArgs> DataChanged;

        private int salary;
        private string title;
        public int Id { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public string passWord { get; set; }
        public int Salary 
        {
            get { return salary; }
            set
            {
                if(value < 0 || value > 1E7 || salary == value)
                {
                    return;
                }
                OnSalaryChanged(EventArgs.Empty);
                OnDataChanged(new EmployeeDataChangedEventArgs("Salary") {Name = this.Name, preValue = salary.ToString(), Value = value.ToString() });
                salary = value;
            }
        }
        public DateTime DateofBirth { get; set; } = DateTime.Now.AddYears(-30);
        public string Title { get => title;
            set
            {
                if (value.Length < 2 || title == value)
                {
                    return;
                }
                OnDataChanged(new EmployeeDataChangedEventArgs("Title") { Name = this.Name, preValue = title, Value = value });
                title = value;
            }
        }

        protected virtual void OnSalaryChanged(EventArgs e)
        {
            EventHandler handler = salaryChanged;
            handler?.Invoke(this, e);
        }

        protected virtual void OnDataChanged(EmployeeDataChangedEventArgs e)
        {
            EventHandler<EmployeeDataChangedEventArgs> handler = DataChanged;
            handler?.Invoke(this, e);
        }
    }

    public class EmployeeDataChangedEventArgs : EventArgs
    {
        public EmployeeDataChangedEventArgs(string fieldChanged)
        {
            this.fieldChanged = fieldChanged;
            changeTime = DateTime.Now;
        }
        public string Name { get; set; }
        public string fieldChanged { get; set; }
        public string preValue { get; set; }
        public string Value { get; set; }
        public DateTime changeTime { get; set; }
        public string changeMessage { get =>  Name + "'s " + fieldChanged + " changed from " + preValue + " to " + Value + " at " + changeTime.ToString();  }
    }

}
