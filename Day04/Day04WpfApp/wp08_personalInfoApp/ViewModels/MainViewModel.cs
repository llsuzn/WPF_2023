﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wp08_personalInfoApp.ViewModels
{
    internal class MainViewModel :ViewModelBase
    {
        // View에서 사용할 멤버변수 선언
        private string inFirstName;
        private string inLastName;
        private string inEmail;
        private DateTime inDate;

        // 결과 출력쪽 변수
        private string outFirstName;
        private string outLastName;
        private string outEmail;
        private string outDate;     // 생일 출력할 땐 문자열로 대체
        private string outAdult;
        private string outBirthDay;
        private string outZodiac;

        // 실제로 사용할 속성
        #region <입력을 위한 속성>
        public string InFirstName
        {
            get => inFirstName;
            set
            {
                inFirstName = value;
                RaisePropertyChanged(nameof(InFirstName));  // "InFirstName"
            }
        }

        public string InLastName 
        { 
            get => inLastName; 
            set
            {
                inLastName = value;
                RaisePropertyChanged(nameof(inLastName)); 
            }
        }
        public string InEmail 
        { 
            get => inEmail; 
            set
            {
                inEmail = value;
                RaisePropertyChanged(nameof(InEmail));
            }
        }
        public DateTime InDate 
        { 
            get => inDate;
            set
            {
                inDate = value;
                RaisePropertyChanged(nameof(InDate));
            }
        }
        #endregion

        #region <출력을 위한 속성>
        public string OutFirstName 
        { 
            get => outFirstName;
            set
            {
                outFirstName = value;
                RaisePropertyChanged(nameof(OutFirstName));
            }
        }
        public string OutLastName 
        { 
            get => outLastName;
            set
            {
                outLastName = value;
                RaisePropertyChanged(nameof(OutLastName));
            }
        }
        public string OutEmail 
        { 
            get => outEmail;
            set
            {
                outEmail = value;
                RaisePropertyChanged(nameof(OutEmail));
            }
        }
        public string OutDate 
        { 
            get => outDate;
            set
            {
                outDate = value;
                RaisePropertyChanged(nameof(OutDate));
            }
        }
        public string OutAdult 
        { 
            get => outAdult;
            set
            {
                outAdult = value;
                RaisePropertyChanged(nameof(OutAdult));
            }
        }
        public string OutBirthDay 
        { 
            get => outBirthDay;
            set
            {
                outBirthDay = value;
                RaisePropertyChanged(nameof(OutBirthDay));
            }
        }
        public string OutZodiac 
        { 
            get => outZodiac;
            set
            {
                outZodiac = value;
                RaisePropertyChanged(nameof(OutZodiac));
            }
        }
        #endregion
    }
}
