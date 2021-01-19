CREATE OR ALTER PROCEDURE dbo.test_class_get_by_id
	@id INT
AS
BEGIN
	SELECT
		[Id] = [id],
		[TestString] = [test_string]
	FROM dbo.test_class
	WHERE [id] = @id
END