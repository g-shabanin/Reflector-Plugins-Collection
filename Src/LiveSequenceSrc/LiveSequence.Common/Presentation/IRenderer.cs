using LiveSequence.Common.Domain;

namespace LiveSequence.Common.Presentation
{
    public interface IRenderer
    {
        string Export(SequenceData data);
    }
}