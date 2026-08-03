using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Wpf.Ui.Violeta.Controls.ColorPickerHelpers;

/// <summary>
/// Contains internal, special-purpose helpers used with the color picker.
/// </summary>
internal static class ColorPickerHelpers
{
    /// <summary>
    /// Generates bitmap pixel data by sweeping a specific color component.
    /// </summary>
    public static Task CreateComponentBitmapAsync(
        byte[] bgraPixelData,
        int width,
        int height,
        Orientation orientation,
        ColorModel colorModel,
        ColorComponent component,
        HsvColor baseHsvColor,
        bool isAlphaVisible,
        bool isPerceptive)
    {
        if (width == 0 || height == 0)
            return Task.CompletedTask;

        return Task.Run(() =>
        {
            int pixelDataIndex = 0;
            double componentStep;
            Color baseRgbColor = Colors.White;
            Color rgbColor;
            int bgraPixelDataWidth = width * 4;

            if (!isAlphaVisible && component != ColorComponent.Alpha)
                baseHsvColor = new HsvColor(1.0, baseHsvColor.H, baseHsvColor.S, baseHsvColor.V);

            if (colorModel == ColorModel.Rgba)
                baseRgbColor = baseHsvColor.ToRgb();

            if (isPerceptive && component != ColorComponent.Alpha)
            {
                if (colorModel == ColorModel.Hsva)
                {
                    switch (component)
                    {
                        case ColorComponent.Component1:
                            baseHsvColor = new HsvColor(baseHsvColor.A, baseHsvColor.H, 1.0, 1.0);
                            break;
                        case ColorComponent.Component2:
                            baseHsvColor = new HsvColor(baseHsvColor.A, baseHsvColor.H, baseHsvColor.S, 1.0);
                            break;
                        case ColorComponent.Component3:
                            baseHsvColor = new HsvColor(baseHsvColor.A, baseHsvColor.H, 1.0, baseHsvColor.V);
                            break;
                    }
                }
                else
                {
                    switch (component)
                    {
                        case ColorComponent.Component1:
                            baseRgbColor = Color.FromArgb(baseRgbColor.A, baseRgbColor.R, 0, 0);
                            break;
                        case ColorComponent.Component2:
                            baseRgbColor = Color.FromArgb(baseRgbColor.A, 0, baseRgbColor.G, 0);
                            break;
                        case ColorComponent.Component3:
                            baseRgbColor = Color.FromArgb(baseRgbColor.A, 0, 0, baseRgbColor.B);
                            break;
                    }
                }
            }

            if (orientation == Orientation.Horizontal)
            {
                if (colorModel == ColorModel.Hsva)
                    componentStep = component == ColorComponent.Component1 ? 360.0 / width : 1.0 / width;
                else
                    componentStep = 255.0 / width;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (y == 0)
                        {
                            rgbColor = GetColor(x * componentStep);
                            bgraPixelData[pixelDataIndex + 0] = Convert.ToByte(rgbColor.B * rgbColor.A / 255);
                            bgraPixelData[pixelDataIndex + 1] = Convert.ToByte(rgbColor.G * rgbColor.A / 255);
                            bgraPixelData[pixelDataIndex + 2] = Convert.ToByte(rgbColor.R * rgbColor.A / 255);
                            bgraPixelData[pixelDataIndex + 3] = rgbColor.A;
                        }
                        else
                        {
                            bgraPixelData[pixelDataIndex + 0] = bgraPixelData[pixelDataIndex + 0 - bgraPixelDataWidth];
                            bgraPixelData[pixelDataIndex + 1] = bgraPixelData[pixelDataIndex + 1 - bgraPixelDataWidth];
                            bgraPixelData[pixelDataIndex + 2] = bgraPixelData[pixelDataIndex + 2 - bgraPixelDataWidth];
                            bgraPixelData[pixelDataIndex + 3] = bgraPixelData[pixelDataIndex + 3 - bgraPixelDataWidth];
                        }

                        pixelDataIndex += 4;
                    }
                }
            }
            else
            {
                if (colorModel == ColorModel.Hsva)
                    componentStep = component == ColorComponent.Component1 ? 360.0 / height : 1.0 / height;
                else
                    componentStep = 255.0 / height;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (x == 0)
                        {
                            rgbColor = GetColor((height - 1 - y) * componentStep);
                            bgraPixelData[pixelDataIndex + 0] = Convert.ToByte(rgbColor.B * rgbColor.A / 255);
                            bgraPixelData[pixelDataIndex + 1] = Convert.ToByte(rgbColor.G * rgbColor.A / 255);
                            bgraPixelData[pixelDataIndex + 2] = Convert.ToByte(rgbColor.R * rgbColor.A / 255);
                            bgraPixelData[pixelDataIndex + 3] = rgbColor.A;
                        }
                        else
                        {
                            bgraPixelData[pixelDataIndex + 0] = bgraPixelData[pixelDataIndex - 4];
                            bgraPixelData[pixelDataIndex + 1] = bgraPixelData[pixelDataIndex - 3];
                            bgraPixelData[pixelDataIndex + 2] = bgraPixelData[pixelDataIndex - 2];
                            bgraPixelData[pixelDataIndex + 3] = bgraPixelData[pixelDataIndex - 1];
                        }

                        pixelDataIndex += 4;
                    }
                }
            }

            Color GetColor(double componentValue)
            {
                Color newRgbColor = Colors.White;

                switch (component)
                {
                    case ColorComponent.Component1:
                        if (colorModel == ColorModel.Hsva)
                        {
                            newRgbColor = HsvColor.ToRgb(
                                Clamp(componentValue, 0.0, 360.0),
                                baseHsvColor.S,
                                baseHsvColor.V,
                                baseHsvColor.A);
                        }
                        else
                        {
                            newRgbColor = Color.FromArgb(
                                baseRgbColor.A,
                                Convert.ToByte(Clamp(componentValue, 0.0, 255.0)),
                                baseRgbColor.G,
                                baseRgbColor.B);
                        }
                        break;

                    case ColorComponent.Component2:
                        if (colorModel == ColorModel.Hsva)
                        {
                            newRgbColor = HsvColor.ToRgb(
                                baseHsvColor.H,
                                Clamp(componentValue, 0.0, 1.0),
                                baseHsvColor.V,
                                baseHsvColor.A);
                        }
                        else
                        {
                            newRgbColor = Color.FromArgb(
                                baseRgbColor.A,
                                baseRgbColor.R,
                                Convert.ToByte(Clamp(componentValue, 0.0, 255.0)),
                                baseRgbColor.B);
                        }
                        break;

                    case ColorComponent.Component3:
                        if (colorModel == ColorModel.Hsva)
                        {
                            newRgbColor = HsvColor.ToRgb(
                                baseHsvColor.H,
                                baseHsvColor.S,
                                Clamp(componentValue, 0.0, 1.0),
                                baseHsvColor.A);
                        }
                        else
                        {
                            newRgbColor = Color.FromArgb(
                                baseRgbColor.A,
                                baseRgbColor.R,
                                baseRgbColor.G,
                                Convert.ToByte(Clamp(componentValue, 0.0, 255.0)));
                        }
                        break;

                    case ColorComponent.Alpha:
                        if (colorModel == ColorModel.Hsva)
                        {
                            newRgbColor = HsvColor.ToRgb(
                                baseHsvColor.H,
                                baseHsvColor.S,
                                baseHsvColor.V,
                                Clamp(componentValue, 0.0, 1.0));
                        }
                        else
                        {
                            newRgbColor = Color.FromArgb(
                                Convert.ToByte(Clamp(componentValue, 0.0, 255.0)),
                                baseRgbColor.R,
                                baseRgbColor.G,
                                baseRgbColor.B);
                        }
                        break;
                }

                return newRgbColor;
            }
        });
    }

    public static Hsv IncrementColorComponent(
        Hsv originalHsv,
        HsvComponent component,
        IncrementDirection direction,
        IncrementAmount amount,
        bool shouldWrap,
        double minBound,
        double maxBound)
    {
        Hsv newHsv = originalHsv;

        if (amount == IncrementAmount.Small || !ColorHelper.ToDisplayNameExists)
        {
            newHsv.S *= 100;
            newHsv.V *= 100;

            double incrementAmount;
            double previousValue;
            double valueToIncrement;

            switch (component)
            {
                case HsvComponent.Hue:
                    valueToIncrement = newHsv.H;
                    incrementAmount = amount == IncrementAmount.Small ? 1 : 30;
                    previousValue = valueToIncrement;
                    valueToIncrement += direction == IncrementDirection.Lower ? -incrementAmount : incrementAmount;
                    if (valueToIncrement < minBound)
                        valueToIncrement = shouldWrap && previousValue == minBound ? maxBound : minBound;
                    if (valueToIncrement > maxBound)
                        valueToIncrement = shouldWrap && previousValue == maxBound ? minBound : maxBound;
                    newHsv.H = valueToIncrement;
                    break;

                case HsvComponent.Saturation:
                    valueToIncrement = newHsv.S;
                    incrementAmount = amount == IncrementAmount.Small ? 1 : 10;
                    previousValue = valueToIncrement;
                    valueToIncrement += direction == IncrementDirection.Lower ? -incrementAmount : incrementAmount;
                    if (valueToIncrement < minBound)
                        valueToIncrement = shouldWrap && previousValue == minBound ? maxBound : minBound;
                    if (valueToIncrement > maxBound)
                        valueToIncrement = shouldWrap && previousValue == maxBound ? minBound : maxBound;
                    newHsv.S = valueToIncrement;
                    break;

                case HsvComponent.Value:
                    valueToIncrement = newHsv.V;
                    incrementAmount = amount == IncrementAmount.Small ? 1 : 10;
                    previousValue = valueToIncrement;
                    valueToIncrement += direction == IncrementDirection.Lower ? -incrementAmount : incrementAmount;
                    if (valueToIncrement < minBound)
                        valueToIncrement = shouldWrap && previousValue == minBound ? maxBound : minBound;
                    if (valueToIncrement > maxBound)
                        valueToIncrement = shouldWrap && previousValue == maxBound ? minBound : maxBound;
                    newHsv.V = valueToIncrement;
                    break;

                default:
                    throw new InvalidOperationException("Invalid HsvComponent.");
            }

            newHsv.S /= 100;
            newHsv.V /= 100;
        }
        else
        {
            if (component is HsvComponent.Saturation or HsvComponent.Value)
            {
                minBound /= 100;
                maxBound /= 100;
            }

            newHsv = FindNextNamedColor(originalHsv, component, direction, shouldWrap, minBound, maxBound);
        }

        return newHsv;
    }

    public static Hsv FindNextNamedColor(
        Hsv originalHsv,
        HsvComponent component,
        IncrementDirection direction,
        bool shouldWrap,
        double minBound,
        double maxBound)
    {
        Hsv newHsv = originalHsv;
        string originalColorName = ColorHelper.ToDisplayName(originalHsv.ToRgb().ToColor());
        string newColorName = originalColorName;

        double originalValue;
        double incrementAmount;

        switch (component)
        {
            case HsvComponent.Hue:
                originalValue = originalHsv.H;
                incrementAmount = 1;
                break;
            case HsvComponent.Saturation:
                originalValue = originalHsv.S;
                incrementAmount = 0.01;
                break;
            case HsvComponent.Value:
                originalValue = originalHsv.V;
                incrementAmount = 0.01;
                break;
            default:
                throw new InvalidOperationException("Invalid HsvComponent.");
        }

        bool shouldFindMidPoint = true;

        while (newColorName == originalColorName)
        {
            double previousValue = GetComponent(newHsv, component);
            double newValue = previousValue + (direction == IncrementDirection.Lower ? -1 : 1) * incrementAmount;
            bool justWrapped = false;

            if (newValue > maxBound)
            {
                if (shouldWrap)
                {
                    newValue = minBound;
                    justWrapped = true;
                }
                else
                {
                    newValue = maxBound;
                    shouldFindMidPoint = false;
                    SetComponent(ref newHsv, component, newValue);
                    newColorName = ColorHelper.ToDisplayName(newHsv.ToRgb().ToColor());
                    break;
                }
            }
            else if (newValue < minBound)
            {
                if (shouldWrap)
                {
                    newValue = maxBound;
                    justWrapped = true;
                }
                else
                {
                    newValue = minBound;
                    shouldFindMidPoint = false;
                    SetComponent(ref newHsv, component, newValue);
                    newColorName = ColorHelper.ToDisplayName(newHsv.ToRgb().ToColor());
                    break;
                }
            }

            SetComponent(ref newHsv, component, newValue);

            if (!justWrapped &&
                previousValue != originalValue &&
                Math.Sign(newValue - originalValue) != Math.Sign(previousValue - originalValue))
            {
                shouldFindMidPoint = false;
                break;
            }

            newColorName = ColorHelper.ToDisplayName(newHsv.ToRgb().ToColor());
        }

        if (shouldFindMidPoint)
        {
            Hsv startHsv = newHsv;
            Hsv currentHsv = startHsv;
            double startEndOffset = 0;
            string currentColorName = newColorName;
            double wrapIncrement = component == HsvComponent.Hue ? 360.0 : 1.0;

            while (newColorName == currentColorName)
            {
                double currentValue = GetComponent(currentHsv, component) +
                    (direction == IncrementDirection.Lower ? -1 : 1) * incrementAmount;

                if (currentValue > maxBound)
                {
                    if (shouldWrap)
                    {
                        currentValue = minBound;
                        startEndOffset = maxBound - minBound;
                    }
                    else
                    {
                        currentValue = maxBound;
                        SetComponent(ref currentHsv, component, currentValue);
                        break;
                    }
                }
                else if (currentValue < minBound)
                {
                    if (shouldWrap)
                    {
                        currentValue = maxBound;
                        startEndOffset = minBound - maxBound;
                    }
                    else
                    {
                        currentValue = minBound;
                        SetComponent(ref currentHsv, component, currentValue);
                        break;
                    }
                }

                SetComponent(ref currentHsv, component, currentValue);
                currentColorName = ColorHelper.ToDisplayName(currentHsv.ToRgb().ToColor());
            }

            double newValue = (GetComponent(startHsv, component) + GetComponent(currentHsv, component) + startEndOffset) / 2;
            double leftoverValue = Math.Abs(newValue);
            while (leftoverValue > incrementAmount)
                leftoverValue -= incrementAmount;
            newValue -= leftoverValue;

            while (newValue < minBound)
                newValue += wrapIncrement;
            while (newValue > maxBound)
                newValue -= wrapIncrement;

            SetComponent(ref newHsv, component, newValue);
        }

        return newHsv;
    }

    /// <summary>
    /// Converts raw BGRA premultiplied pixel data into a WPF <see cref="WriteableBitmap"/>.
    /// </summary>
    public static WriteableBitmap CreateBitmapFromPixelData(byte[] bgraPixelData, int pixelWidth, int pixelHeight)
    {
        var bitmap = new WriteableBitmap(pixelWidth, pixelHeight, 96, 96, PixelFormats.Bgra32, null);
        bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, pixelWidth, pixelHeight), bgraPixelData, pixelWidth * 4, 0);
        return bitmap;
    }

    private static double GetComponent(Hsv hsv, HsvComponent component) => component switch
    {
        HsvComponent.Hue => hsv.H,
        HsvComponent.Saturation => hsv.S,
        HsvComponent.Value => hsv.V,
        _ => throw new InvalidOperationException("Invalid HsvComponent."),
    };

    private static void SetComponent(ref Hsv hsv, HsvComponent component, double value)
    {
        switch (component)
        {
            case HsvComponent.Hue: hsv.H = value; break;
            case HsvComponent.Saturation: hsv.S = value; break;
            case HsvComponent.Value: hsv.V = value; break;
            default: throw new InvalidOperationException("Invalid HsvComponent.");
        }
    }

    private static double Clamp(double value, double min, double max) =>
        value < min ? min : value > max ? max : value;
}
