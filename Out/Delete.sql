CREATE OR ALTER PROCEDURE dbo.test_class_delete
	@id INT
AS
BEGIN
	DELETE
	FROM dbo.test_class
	WHERE [id] = @id
END