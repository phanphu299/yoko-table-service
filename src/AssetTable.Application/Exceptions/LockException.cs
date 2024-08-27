using AHI.Infrastructure.Exception;

namespace AssetTable.Application.Exception
{
    public static class LockExceptionHelper
    {
        public static EntityNotLockException CreateNotLockException()
        {
            return new EntityNotLockException();
        }

        public static EntityAlreadyLockException CreateAlreadyLockException()
        {
            return new EntityAlreadyLockException();
        }
        public static EntityLockRequestInProcessException CreateAlreadySendRequestUnlockException()
        {
            return new EntityLockRequestInProcessException();
        }

        public static EntityLockException CreateLockByOtherException()
        {
            return new EntityLockException();
        }

        public static EntityLockTakenByOtherException CreateRequestTakeLockByOtherException()
        {
            return new EntityLockTakenByOtherException();
        }
    }
}
