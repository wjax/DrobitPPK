using ControlCenter.Extras;
using ControlCenter.Models.JOBS;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace ControlCenter.Converters
{
    public class LongTimeToNiceStringTimeConverter : MarkupExtension, IValueConverter
    {
        private static LongTimeToNiceStringTimeConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new LongTimeToNiceStringTimeConverter();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long time = (long)value;
            return DrobitTools.fromMillisNice(time);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ValidTime2ColorConverter : MarkupExtension, IValueConverter
    {
        private static ValidTime2ColorConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new ValidTime2ColorConverter();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool valid = (bool)value;
            Brush colorTime = valid ? Brushes.Green : Brushes.Red;

            return colorTime;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsRecordingToString : MarkupExtension, IValueConverter
    {
        private static IsRecordingToString _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new IsRecordingToString();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isRec = (bool)value;
            if (isRec)
                return "STOP";
            else
                return "REC";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibility : MarkupExtension, IValueConverter
    {
        private static BooleanToVisibility _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new BooleanToVisibility();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            bool isVisible = (bool)value;
            if (isVisible)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InvertBoolean : MarkupExtension, IValueConverter
    {
        private static InvertBoolean _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new InvertBoolean();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(GNSSProcessingTYPE), typeof(Visibility))]
    public class GNSSProcessingType2TrueVisibilityIfKinematic : MarkupExtension, IValueConverter
    {
        private static GNSSProcessingType2TrueVisibilityIfKinematic _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new GNSSProcessingType2TrueVisibilityIfKinematic();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GNSSProcessingTYPE type = (GNSSProcessingTYPE)value;
            if (type == GNSSProcessingTYPE.KINEMATIC)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(GNSSProcessingTYPE), typeof(Visibility))]
    public class GNSSProcessingType2TrueVisibilityIfStatic : MarkupExtension, IValueConverter
    {
        private static GNSSProcessingType2TrueVisibilityIfStatic _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new GNSSProcessingType2TrueVisibilityIfStatic();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GNSSProcessingTYPE type = (GNSSProcessingTYPE)value;
            if (type == GNSSProcessingTYPE.KINEMATIC)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class GNSSToLocation : MarkupExtension, IValueConverter
    //{
    //    private static GNSSToLocation _converter = null;
    //    public override object ProvideValue(IServiceProvider serviceProvider)
    //    {
    //        if (_converter == null)
    //        {
    //            _converter = new GNSSToLocation();
    //        }
    //        return _converter;
    //    }

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        Location l = new Location();
    //        GNSSStatus a = (GNSSStatus)value;
    //        l.Latitude = a.Lat;
    //        l.Longitude = a.Lon;
    //        l.Altitude = a.Alt;
    //        l.AltitudeReference = AltitudeReference.Ellipsoid;

    //        return l;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    public class BytesToNiceString : MarkupExtension, IValueConverter
    {
        private static BytesToNiceString _converter = null;

        static readonly string[] SizeSuffixes =
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        static string SizeSuffix(Int64 value)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new BytesToNiceString();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long bytes = (long)value;


            return SizeSuffix(bytes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(bool))]
    public class Text2Boolean : MarkupExtension, IValueConverter
    {
        private static Text2Boolean _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new Text2Boolean();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool toReturn = false;
            string sValue = (string)value;

            if (sValue.ToLowerInvariant().Equals("true"))
                toReturn = true;

            return toReturn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bValue = (bool)value;
            return bValue.ToString();
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class Text2TextFiltered : MarkupExtension, IValueConverter
    {
        private static Text2TextFiltered _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new Text2TextFiltered();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string toReturn = "";
            string sValue = (string)value;
            double dummy;

            if (double.TryParse(sValue, NumberStyles.Any, CultureInfo.InvariantCulture, out dummy))
                toReturn = sValue;
            else
                toReturn = (string)parameter;

            return toReturn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)value;
        }
    }

    [ValueConversion(typeof(WorkPhase), typeof(bool))]
    public class WorkPhase2AllowModifications : MarkupExtension, IValueConverter
    {
        private static WorkPhase2AllowModifications _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new WorkPhase2AllowModifications();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bEnabled = true;
            if (value is WorkPhase)
            {
                WorkPhase phase = (WorkPhase)value;
                if (phase == WorkPhase.RUNNING || phase == WorkPhase.CANCELLING)
                    bEnabled = false;
            }


            return bEnabled;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)value;
        }
    }

    public class Enum2Bool : MarkupExtension, IValueConverter
    {
        private static Enum2Bool _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new Enum2Bool();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }

    }

    public class Null2Bool : MarkupExtension, IValueConverter
    {
        private static Null2Bool _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new Null2Bool();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;
            else
                return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    public class AssemblyVersionConverter : MarkupExtension, IValueConverter
    {
        private static AssemblyVersionConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new AssemblyVersionConverter();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                Version v = value as Version;

                return v.Major + "." + v.Minor;
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    public class TextClearConverter : MarkupExtension, IMultiValueConverter
    {
        private static TextClearConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new TextClearConverter();
            }
            return _converter;
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (values != null && values.Length == 2 && values[0] is bool && values[1] is string)
            {
                bool isRecording = (bool)values[0];
                //string currText = (string)values[1];
                string actualRecName = (string)values[1];

                if (isRecording)
                    return actualRecName;
                else
                    return "";
            }

            return "";

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }


    public class PowerEnabledConverter : MarkupExtension, IMultiValueConverter
    {
        private static PowerEnabledConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new PowerEnabledConverter();
            }
            return _converter;
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = false;

            if (values == null)
                return false;

            try
            {
                bool canbetoggled = (bool)values[0];
                bool isRecording = (bool)values[1];

                if (canbetoggled && !isRecording)
                    result = true;

            }
            finally
            {
                
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    public class Phase2IconKind : MarkupExtension, IValueConverter
    {
        private static Phase2IconKind _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new Phase2IconKind();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            WorkPhase phase = (WorkPhase)value;
            MaterialDesignThemes.Wpf.PackIcon icon = new MaterialDesignThemes.Wpf.PackIcon();

            switch (phase)
            {
                case WorkPhase.COMPLETED:
                    icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                    break;
                case WorkPhase.FAILED:
                    icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                    break;
                case WorkPhase.RUNNING:
                    icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Stop;
                    break;
                case WorkPhase.STOP:
                    icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                    break;
                case WorkPhase.CANCELLING:
                    icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Stop;
                    break;
                case WorkPhase.UNDEF:
                    icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                    break;
                default:
                    icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                    break;
            }

            return icon;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    [ValueConversion(typeof(int), typeof(string))]
    public class BattPercent2IconKind : MarkupExtension, IValueConverter
    {
        private static BattPercent2IconKind _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new BattPercent2IconKind();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string bIconKind = "";
            int bp = (int)value;

            if (bp < 15)
                bIconKind = "Battery10";
            else if (bp < 25)
                bIconKind = "Battery20";
            else if (bp < 35)
                bIconKind = "Battery30";
            else if (bp < 45)
                bIconKind = "Battery40";
            else if (bp < 55)
                bIconKind = "Battery50";
            else if (bp < 65)
                bIconKind = "Battery60";
            else if (bp < 75)
                bIconKind = "Battery70";
            else if (bp < 85)
                bIconKind = "Battery80";
            else if (bp < 95)
                bIconKind = "Battery90";
            else
                bIconKind = "Battery";

            return bIconKind;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)value;
        }
    }

    //[ValueConversion(typeof(string), typeof(IMU_ROTATION))]
    //public class IMUROTATION2String : MarkupExtension, IValueConverter
    //{
    //    private static IMUROTATION2String _converter = null;
    //    public override object ProvideValue(IServiceProvider serviceProvider)
    //    {
    //        if (_converter == null)
    //        {
    //            _converter = new IMUROTATION2String();
    //        }
    //        return _converter;
    //    }

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value == null)
    //            return IMU_ROTATION.ROTATION_NONE;

    //        IMU_ROTATION rot = (IMU_ROTATION)Enum.Parse(typeof(IMU_ROTATION), value as string);
    //        return rot;

    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value == null)
    //            return "ROTATION_NONE";

    //        IMU_ROTATION rot = (IMU_ROTATION)value;
    //        return rot.ToString();
    //    }
    //}

    // MultiValueConverter
    public class MultiValueConverter : MarkupExtension, IMultiValueConverter
    {
        private static MultiValueConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new MultiValueConverter();
            }
            return _converter;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MultiControlViewEnabled : MarkupExtension, IMultiValueConverter
    {
        private static MultiControlViewEnabled _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new MultiControlViewEnabled();
            }
            return _converter;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                object isNull = values[0] as object;
                bool boardActive = (bool)values[1];
                bool isRecording = (bool)values[2];

                if (isNull == null || boardActive == false || isRecording == true)
                    return false;
                else
                    return true;
            }
            catch (Exception e)
            {
                return false;
            }
            
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
