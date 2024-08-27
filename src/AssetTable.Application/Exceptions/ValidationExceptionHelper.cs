namespace AHI.Infrastructure.Exception.Helper
{
    public static class ValidationExceptionHelper
    {
        public static EntityValidationException GenerateNotFoundValidation(string fieldName)
        {
            return EntityValidationExceptionHelper.GenerateException(
                fieldName,
                ExceptionErrorCode.DetailCode.ERROR_VALIDATION_NOT_FOUND,
                detailCode: ExceptionErrorCode.DetailCode.ERROR_VALIDATION_SOME_ITEMS_DELETED);
        }

        public static EntityValidationException GenerateDuplicateValidation(string fieldName)
        {
            return EntityValidationExceptionHelper.GenerateException(
                fieldName,
                ExceptionErrorCode.DetailCode.ERROR_VALIDATION_DUPLICATED);
        }

        public static EntityValidationException GenerateRequiredValidation(string fieldName)
        {
            return EntityValidationExceptionHelper.GenerateException(
                fieldName,
                ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED);
        }

        public static EntityValidationException GenerateInvalidValidation(string fieldName)
        {
            return EntityValidationExceptionHelper.GenerateException(
                fieldName,
                ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID);
        }
    }
}