using ControlCenter.Models;
using SharpKml.Base;
using SharpKml.Dom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using static ControlCenter.Models.JOBS.CAMProcessingJob;

namespace ControlCenter.Extras.KML
{
    public class KMLCreator
    {
        private static Style cameraStyle;

        static KMLCreator()
        {
            // Create the style 
            cameraStyle = new Style();
            cameraStyle.Id = "CameraStyle";
            cameraStyle.Icon = new IconStyle();
            //style.Icon.Color = new Color32(255, 0, 255, 0);
            //style.Icon.ColorMode = ColorMode.Normal;
            Uri uri = new Uri(@"icons/camera.png", UriKind.Relative);
            cameraStyle.Icon.Icon = new IconStyle.IconLink(uri);
            cameraStyle.Icon.Scale = 1;
        }

        public static void CreateKML3D(SortedDictionary<long, CameraShot> items, string folder, string filename, string altitudeOffset, string modelScale, bool UseBodyIMU, string pitchOffset)
        {
            double dAltitudeOffset = 0;
            if (!double.TryParse(altitudeOffset, NumberStyles.Any, CultureInfo.InvariantCulture, out dAltitudeOffset))
                dAltitudeOffset = 0;

            double dModelScale = 1;
            if (!double.TryParse(modelScale, NumberStyles.Any, CultureInfo.InvariantCulture, out dModelScale))
                dModelScale = 1;

            double dPitchOffset = 0;
            if (!double.TryParse(pitchOffset, NumberStyles.Any, CultureInfo.InvariantCulture, out dPitchOffset))
                dPitchOffset = 0;

            var filePath = DrobitTools.ConcatenatePath(new string[] { folder, "CameraPositions.kml" });

            File.Copy(DrobitTools.AbsolutizePath("resources/camera.png"), DrobitTools.ConcatenatePath(new string[] { folder, "kmlresources", "camera.png" }));
            File.Copy(DrobitTools.AbsolutizePath("resources/camera3d.dae"), DrobitTools.ConcatenatePath(new string[] { folder, "kmlresources", "camera3d.dae" }));
            File.Copy(DrobitTools.AbsolutizePath("resources/DrobitGreen.dae"), DrobitTools.ConcatenatePath(new string[] { folder, "kmlresources", "DrobitGreen.dae" }));
            File.Copy(DrobitTools.AbsolutizePath("resources/DrobitRed.dae"), DrobitTools.ConcatenatePath(new string[] { folder, "kmlresources", "DrobitRed.dae" }));
            File.Copy(DrobitTools.AbsolutizePath("resources/DrobitOrange.dae"), DrobitTools.ConcatenatePath(new string[] { folder, "kmlresources", "DrobitOrange.dae" }));

            // Package it all together...
            var document = new Document();
            // Add Style
            document.AddStyle(cameraStyle);

            foreach (CameraShot c in items.Values)
            {
                // Now create the object to apply the style to
                Placemark placemark = new Placemark();
                placemark.Name = c.Name;
                placemark.Snippet = new Snippet()
                {
                    Text = $"RollBody: {c.RollB}, PitchBody: {c.PitchB}, HeadingBody: {c.YawB}" + Environment.NewLine +
                        $"RollHotshoe: {c.RollH}, PitchHotshoe: {c.PitchH}, HeadingHotshoe: {c.YawH}"
                };
                placemark.Description = new Description()
                {
                    Text = $"RollBody: {c.RollB}, PitchBody: {c.PitchB}, HeadingBody: {c.YawB}" + Environment.NewLine +
                        $"RollHotshoe: {c.RollH}, PitchHotshoe: {c.PitchH}, HeadingHotshoe: {c.YawH}"
                };


                Location loc = new Location();
                loc.Latitude = c.PreciseLatNodalPoint;
                loc.Longitude = c.PreciseLonNodalPoint;
                loc.Altitude = c.PreciseAltNodalPoint + dAltitudeOffset;

             

                Orientation ori = new Orientation();
                if (UseBodyIMU)
                {
                    ori.Roll = c.RollB;
                    ori.Tilt = -(c.PitchB + dPitchOffset);
                    ori.Heading = c.YawB;
                }
                else
                {
                    ori.Roll = c.RollH;
                    ori.Tilt = -(c.PitchH + dPitchOffset);
                    ori.Heading = c.YawH;
                }
              

                Scale sca = new Scale();
                sca.X = sca.Y = sca.Z = dModelScale;

                Link link = new Link();
                if (c.Accuracy <= CameraShot.HIGH_ACCURACY)
                    link.Href = new Uri(@"kmlresources/DrobitGreen.dae", UriKind.Relative);
                else if (c.Accuracy <= CameraShot.MEDIUM_ACCURACY)
                    link.Href = new Uri(@"kmlresources/DrobitOrange.dae", UriKind.Relative);
                else
                    link.Href = new Uri(@"kmlresources/DrobitRed.dae", UriKind.Relative);

                Model cameraModel = new Model();
                cameraModel.Location = loc;
                cameraModel.Orientation = ori;
                cameraModel.Scale = sca;
                cameraModel.AltitudeMode = AltitudeMode.Absolute;
                cameraModel.Link = link;

                placemark.Geometry = cameraModel;

                document.AddFeature(placemark);
            }

            Serializer serializer = new Serializer();
            serializer.Serialize(document);
            File.WriteAllText(filePath, serializer.Xml);
            document.ClearStyles();
        }

        public static void CreateKML(SortedDictionary<long, CameraShot> items, string folder, string filename)
        {
            var filePath = DrobitTools.ConcatenatePath(new string[] { folder, "CameraPositions.kml" });

            File.Copy(DrobitTools.AbsolutizePath("resources/camera.png"), DrobitTools.ConcatenatePath(new string[] { folder, "icons", "camera.png" }));

            // Package it all together...
            var document = new Document();
            // Add Style
            document.AddStyle(cameraStyle);

            foreach (CameraShot c in items.Values)
            {
                // Now create the object to apply the style to
                Placemark placemark = new Placemark();
                placemark.Name = c.Name;
                placemark.StyleUrl = new Uri("#CameraStyle", UriKind.Relative);

                placemark.Geometry = new Point
                {
                    Coordinate = new Vector(c.PreciseLatNodalPoint, c.PreciseLonNodalPoint)
                };

                document.AddFeature(placemark);
            }

            Serializer serializer = new Serializer();
            serializer.Serialize(document);
            File.WriteAllText(filePath, serializer.Xml);
            document.ClearStyles();
        }
    }
}
