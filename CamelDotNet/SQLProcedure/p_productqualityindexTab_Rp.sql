create procedure [dbo].[p_productqualityindexTab_Rp]
	@testtimestart datetime2,
	@testtimestop datetime2,
	@drillingcrew nvarchar(50),--机台
	@workgroup nvarchar(50), --班组
	@producttypeId int,--物料名称
	@clientId int,--客户
	@orderNo nvarchar(50)--订单号
as
begin
	SET NOCOUNT ON;
	--判断@producttypeId，如果为0，置为null
	IF @producttypeId = 0
	BEGIN
		SET @producttypeId = NULL
	END
	--判断@clientId，如果为0，置为null
	IF @clientId = 0
	BEGIN
		SET @clientId = NULL
	END

	declare @VnaInfo table
	(
		VnaRecordId int,
		SerialNumber nvarchar(100),
		ReelNumber nvarchar(50)
	)
	insert into @VnaInfo
		select 
		a.Id as VnaRecordId,
		b.Number as SerialNumber,
		a.ReelNumber
		from VnaRecord a
		join SerialNumber b
		on
		a.SerialNumberId = b.Id
		where
		a.reTest = 0--not reTest or last test
		and
		a.NoStatistics = 0--is statistic
		and
		a.TestTime >= @testtimestart
		and
		a.TestTime <= @testtimestop
		and a.DrillingCrew like '%' + IsNull(@drillingcrew,a.DrillingCrew) +'%'
		and a.WorkGroup like '%' + IsNull(@workgroup,a.workgroup) + '%'
		and a.ProductTypeId = ISNULL(@producttypeId,a.ProductTypeId)
		and a.ClientId = ISNULL(@clientId,a.ClientId)
		and a.OrderNo like '%' + IsNull(@orderNo,a.OrderNo) + '%'

	declare @VnaResult table
	(
		VnaRecordId int,
		SerialNumber nvarchar(100),
		ReelNumber nvarchar(50),
		SWOneCone decimal(18,2),
		SWOneCtwo decimal(18,2),
		SWOneCthree decimal(18,2),
		SWOneCfour decimal(18,2),
		SWTwoCone decimal(18,2),
		SWTwoCtwo decimal(18,2),
		SWTwoCthree decimal(18,2),
		SWTwoCfour decimal(18,2),
		RLOneCone decimal(18,2),
		RLOneCtwo decimal(18,2),
		RLOneCthree decimal(18,2),
		RLOneCfour decimal(18,2),
		RLTwoCone decimal(18,2),
		RLTwoCtwo decimal(18,2),
		RLTwoCthree decimal(18,2),
		RLTwoCfour decimal(18,2),
		Attenuation100 decimal(18,2),
		Attenuation150 decimal(18,2),
		Attenuation200 decimal(18,2),
		Attenuation280 decimal(18,2),
		Attenuation450 decimal(18,2),
		Attenuation800 decimal(18,2),
		Attenuation900 decimal(18,2),
		Attenuation1000 decimal(18,2),
		Attenuation1500 decimal(18,2),
		Attenuation1800 decimal(18,2),
		Attenuation2000 decimal(18,2),
		Attenuation2200 decimal(18,2),
		Attenuation2400 decimal(18,2),
		Attenuation2500 decimal(18,2),
		Attenuation3000 decimal(18,2),
		Attenuation3400 decimal(18,2),
		Attenuation4000 decimal(18,2),
		TDI decimal(18,2)
	)

	insert into @VnaResult
		select 
			aaaa.VnaRecordId, 
			aaaa.SerialNumber,
			aaaa.ReelNumber,
			MAX(aaaa.SWOneCone) AS SWOneCone,
			MAX(aaaa.SWOneCtwo) AS SWOneCtwo,
			MAX(aaaa.SWOneCthree) AS SWOneCthree,
			MAX(aaaa.SWOneCfour) AS SWOneCfour,
			MAX(aaaa.SWTwoCone) AS SWTwoCone,
			MAX(aaaa.SWTwoCtwo) AS SWTwoCtwo,
			MAX(aaaa.SWTwoCthree) AS SWTwoCthree,
			MAX(aaaa.SWTwoCfour) AS SWTwoCfour,
			MAX(aaaa.RLOneCone) AS RLOneCone,
			MAX(aaaa.RLOneCtwo) AS RLOneCtwo,
			MAX(aaaa.RLOneCthree) AS RLOneCthree,
			MAX(aaaa.RLOneCfour) AS RLOneCfour,
			MAX(aaaa.RLTwoCone) AS RLTwoCone,
			MAX(aaaa.RLTwoCtwo) AS RLTwoCtwo,
			MAX(aaaa.RLTwoCthree) AS RLTwoCthree,
			MAX(aaaa.RLTwoCfour) AS RLTwoCfour,
			MAX(aaaa.Attenuation100) AS Attenuation100,
			MAX(aaaa.Attenuation150) AS Attenuation150,
			MAX(aaaa.Attenuation200) AS Attenuation200,
			MAX(aaaa.Attenuation280) AS Attenuation280,
			MAX(aaaa.Attenuation450) AS Attenuation450,
			MAX(aaaa.Attenuation800) AS Attenuation800,
			MAX(aaaa.Attenuation900) AS Attenuation900,
			MAX(aaaa.Attenuation1000) AS Attenuation1000,
			MAX(aaaa.Attenuation1500) AS Attenuation1500,
			MAX(aaaa.Attenuation1800) AS Attenuation1800,
			MAX(aaaa.Attenuation2000) AS Attenuation2000,
			MAX(aaaa.Attenuation2200) AS Attenuation2200,
			MAX(aaaa.Attenuation2400) AS Attenuation2400,
			MAX(aaaa.Attenuation2500) AS Attenuation2500,
			MAX(aaaa.Attenuation3000) AS Attenuation3000,
			MAX(aaaa.Attenuation3400) AS Attenuation3400,
			MAX(aaaa.Attenuation4000) AS Attenuation4000,
			MAX(aaaa.TDI) AS TDI
		from
		(
			select 
				aaa.VnaRecordId,aaa.SerialNumber,aaa.ReelNumber,
				case
					when aaa.TestitemName = '驻波1' and aaa.RowNumber = 1 then ABS(aaa.YValue) else null
				end SWOneCone,
				case
					when aaa.TestitemName = '驻波1' and aaa.RowNumber = 2 then ABS(aaa.YValue) else null
				end SWOneCtwo,
				case
					when aaa.TestitemName = '驻波1' and aaa.RowNumber = 3 then ABS(aaa.YValue) else null
				end SWOneCthree,
				case
					when aaa.TestitemName = '驻波1' and aaa.RowNumber = 4 then ABS(aaa.YValue) else null
				end SWOneCfour,
				case
					when aaa.TestitemName = '驻波2' and aaa.RowNumber = 1 then ABS(aaa.YValue) else null
				end SWTwoCone,
				case
					when aaa.TestitemName = '驻波2' and aaa.RowNumber = 2 then ABS(aaa.YValue) else null
				end SWTwoCtwo,
				case
					when aaa.TestitemName = '驻波2' and aaa.RowNumber = 3 then ABS(aaa.YValue) else null
				end SWTwoCthree,
				case
					when aaa.TestitemName = '驻波2' and aaa.RowNumber = 4 then ABS(aaa.YValue) else null
				end SWTwoCfour,
				case
					when aaa.TestitemName = '回波损耗1' and aaa.RowNumber = 1 then ABS(aaa.YValue) else null
				end RLOneCone,
				case
					when aaa.TestitemName = '回波损耗1' and aaa.RowNumber = 2 then ABS(aaa.YValue) else null
				end RLOneCtwo,
				case
					when aaa.TestitemName = '回波损耗1' and aaa.RowNumber = 3 then ABS(aaa.YValue) else null
				end RLOneCthree,
				case
					when aaa.TestitemName = '回波损耗1' and aaa.RowNumber = 4 then ABS(aaa.YValue) else null
				end RLOneCfour,
				case
					when aaa.TestitemName = '回波损耗2' and aaa.RowNumber = 1 then ABS(aaa.YValue) else null
				end RLTwoCone,
				case
					when aaa.TestitemName = '回波损耗2' and aaa.RowNumber = 2 then ABS(aaa.YValue) else null
				end RLTwoCtwo,
				case
					when aaa.TestitemName = '回波损耗2' and aaa.RowNumber = 3 then ABS(aaa.YValue) else null
				end RLTwoCthree,
				case
					when aaa.TestitemName = '回波损耗2' and aaa.RowNumber = 4 then ABS(aaa.YValue) else null
				end RLTwoCfour,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 100000000 then ABS(aaa.RValue) else null
				end Attenuation100,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 150000000 then ABS(aaa.RValue) else null
				end Attenuation150,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 200000000 then ABS(aaa.RValue) else null
				end Attenuation200,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 280000000 then ABS(aaa.RValue) else null
				end Attenuation280,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 450000000 then ABS(aaa.RValue) else null
				end Attenuation450,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 800000000 then ABS(aaa.RValue) else null
				end Attenuation800,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 900000000 then ABS(aaa.RValue) else null
				end Attenuation900,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 1000000000 then ABS(aaa.RValue) else null
				end Attenuation1000,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 1500000000 then ABS(aaa.RValue) else null
				end Attenuation1500,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 1800000000 then ABS(aaa.RValue) else null
				end Attenuation1800,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 2000000000 then ABS(aaa.RValue) else null
				end Attenuation2000,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 2200000000 then ABS(aaa.RValue) else null
				end Attenuation2200,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 2400000000 then ABS(aaa.RValue) else null
				end Attenuation2400,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 2500000000 then ABS(aaa.RValue) else null
				end Attenuation2500,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 3000000000 then ABS(aaa.RValue) else null
				end Attenuation3000,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 3400000000 then ABS(aaa.RValue) else null
				end Attenuation3400,
				case 
					when aaa.TestitemName = '衰减' and aaa.XValue = 4000000000 then ABS(aaa.RValue) else null
				end Attenuation4000,
				case
					when aaa.TestitemName = '时域阻抗' and aaa.RowNumber = 1 then ABS(aaa.RValue) else null
				end TDI
			from 
			(
				select
				aa.*,
				ROW_NUMBER() OVER(PARTITION BY aa.VnaRecordId,aa.TestitemName ORDER BY aa.VnaRecordId) AS RowNumber,
				bb.XValue,bb.YValue,bb.RValue
				from
				(
					select 
						a.VnaRecordId,
						a.SerialNumber,
						a.ReelNumber,
						b.Id as VnaTestItemRecordId,
						b.TestItemId,
						c.Name as TestitemName
					from
					@VnaInfo a
					join VnaTestItemRecord b
					on a.VnaRecordId = b.VnaRecordId
					join TestItem c
					on b.TestItemId = c.Id
					and c.Name in ('驻波1','驻波2','回波损耗1','回波损耗2','衰减','时域阻抗')
				) aa
				join VnaTestItemPerRecord bb
				on aa.VnaTestItemRecordId = bb.VnaTestItemRecordId
			)aaa
			group by aaa.VnaRecordId,aaa.SerialNumber,aaa.ReelNumber,aaa.TestitemName,aaa.XValue,aaa.RowNumber,aaa.YValue,aaa.RValue
		) aaaa
		group by aaaa.VnaRecordId, aaaa.SerialNumber,aaaa.ReelNumber

	select * from @VnaResult

end