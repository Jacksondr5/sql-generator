CREATE OR ALTER PROCEDURE dbo.test_class_update
	@id INT,
	@test_string VARCHAR(50)
AS
BEGIN
	UPDATE dbo.test_class
	SET
		[test_string] = @test_string
	WHERE [id] = @id
END