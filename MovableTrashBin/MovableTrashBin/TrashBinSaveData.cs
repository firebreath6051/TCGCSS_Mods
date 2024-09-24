using System;

namespace MovableTrashBin
{
    [Serializable]
    public class TrashBinSaveData
    {
        public Vector3Serializer pos;

        public QuaternionSerializer rot;

        public EObjectType objectType;
    }
}
