using System.Collections.Generic;
using System.Linq;
using CharacterDataEditor.Models.CharacterData;

namespace CharacterDataEditor.Extensions;

public static class FrameDataModelExtensions
{
    public static FrameDataModel GetFrameToDraw(this List<FrameDataModel> frameData, int currentFrame)
    {
        var validFrames = frameData.Where(x => x.Length >= currentFrame);

        var returnValue = validFrames.FirstOrDefault();

        if (returnValue == null)
        {
            return null;
        }

        foreach (var frame in validFrames)
        {
            if (frame.Length < returnValue.Length)
            {
                returnValue = frame;
            }
        }

        return returnValue;
    }

    public static int GetMaxFrame(this List<FrameDataModel> frameData)
    {
        return frameData.Max(x => x.Length);
    }
}
