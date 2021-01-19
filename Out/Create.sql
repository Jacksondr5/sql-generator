CREATE OR ALTER PROCEDURE dbo.test_class_create
	@test_string VARCHAR(50)
AS
BEGIN
	INSERT dbo.test_class (
		test_string
	)
	VALUES (
		@test_string
	);

	SELECT SCOPE_IDENTITY()
END