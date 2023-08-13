﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.ComponentModel;

namespace InterpolatedSql.Dapper.Tests.DynamicCRUDTests
{
    [Table("Product", Schema = "Production")]
    public partial class Product : INotifyPropertyChanged
    {
        #region Members
        private int _productId;
        [Key]
        public int ProductId
        {
            get { return _productId; }
            set { SetField(ref _productId, value, nameof(ProductId)); }
        }
        private string _class;
        public string Class
        {
            get { return _class; }
            set { SetField(ref _class, value, nameof(Class)); }
        }
        private string _color;
        public string Color
        {
            get { return _color; }
            set { SetField(ref _color, value, nameof(Color)); }
        }
        private int _daysToManufacture;
        public int DaysToManufacture
        {
            get { return _daysToManufacture; }
            set { SetField(ref _daysToManufacture, value, nameof(DaysToManufacture)); }
        }
        private DateTime? _discontinuedDate;
        public DateTime? DiscontinuedDate
        {
            get { return _discontinuedDate; }
            set { SetField(ref _discontinuedDate, value, nameof(DiscontinuedDate)); }
        }
        private bool _finishedGoodsFlag;
        public bool FinishedGoodsFlag
        {
            get { return _finishedGoodsFlag; }
            set { SetField(ref _finishedGoodsFlag, value, nameof(FinishedGoodsFlag)); }
        }
        private decimal _listPrice;
        public decimal ListPrice
        {
            get { return _listPrice; }
            set { SetField(ref _listPrice, value, nameof(ListPrice)); }
        }
        private bool _makeFlag;
        public bool MakeFlag
        {
            get { return _makeFlag; }
            set { SetField(ref _makeFlag, value, nameof(MakeFlag)); }
        }
        private DateTime _modifiedDate;
        public DateTime ModifiedDate
        {
            get { return _modifiedDate; }
            set { SetField(ref _modifiedDate, value, nameof(ModifiedDate)); }
        }
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value, nameof(Name)); }
        }
        private string _productLine;
        public string ProductLine
        {
            get { return _productLine; }
            set { SetField(ref _productLine, value, nameof(ProductLine)); }
        }
        private int? _productModelId;
        public int? ProductModelId
        {
            get { return _productModelId; }
            set { SetField(ref _productModelId, value, nameof(ProductModelId)); }
        }
        private string _productNumber;
        public string ProductNumber
        {
            get { return _productNumber; }
            set { SetField(ref _productNumber, value, nameof(ProductNumber)); }
        }
        private int? _productSubcategoryId;
        public int? ProductSubcategoryId
        {
            get { return _productSubcategoryId; }
            set { SetField(ref _productSubcategoryId, value, nameof(ProductSubcategoryId)); }
        }
        private short _reorderPoint;
        public short ReorderPoint
        {
            get { return _reorderPoint; }
            set { SetField(ref _reorderPoint, value, nameof(ReorderPoint)); }
        }
        private Guid _rowguid;
        public Guid Rowguid
        {
            get { return _rowguid; }
            set { SetField(ref _rowguid, value, nameof(Rowguid)); }
        }
        private short _safetyStockLevel;
        public short SafetyStockLevel
        {
            get { return _safetyStockLevel; }
            set { SetField(ref _safetyStockLevel, value, nameof(SafetyStockLevel)); }
        }
        private DateTime? _sellEndDate;
        public DateTime? SellEndDate
        {
            get { return _sellEndDate; }
            set { SetField(ref _sellEndDate, value, nameof(SellEndDate)); }
        }
        private DateTime _sellStartDate;
        public DateTime SellStartDate
        {
            get { return _sellStartDate; }
            set { SetField(ref _sellStartDate, value, nameof(SellStartDate)); }
        }
        private string _size;
        public string Size
        {
            get { return _size; }
            set { SetField(ref _size, value, nameof(Size)); }
        }
        private string _sizeUnitMeasureCode;
        public string SizeUnitMeasureCode
        {
            get { return _sizeUnitMeasureCode; }
            set { SetField(ref _sizeUnitMeasureCode, value, nameof(SizeUnitMeasureCode)); }
        }
        private decimal _standardCost;
        public decimal StandardCost
        {
            get { return _standardCost; }
            set { SetField(ref _standardCost, value, nameof(StandardCost)); }
        }
        private string _style;
        public string Style
        {
            get { return _style; }
            set { SetField(ref _style, value, nameof(Style)); }
        }
        private decimal? _weight;
        public decimal? Weight
        {
            get { return _weight; }
            set { SetField(ref _weight, value, nameof(Weight)); }
        }
        private string _weightUnitMeasureCode;
        public string WeightUnitMeasureCode
        {
            get { return _weightUnitMeasureCode; }
            set { SetField(ref _weightUnitMeasureCode, value, nameof(WeightUnitMeasureCode)); }
        }
        #endregion Members

        #region Equals/GetHashCode
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            Product other = obj as Product;
            if (other == null) return false;

            if (Class != other.Class)
                return false;
            if (Color != other.Color)
                return false;
            if (DaysToManufacture != other.DaysToManufacture)
                return false;
            if (DiscontinuedDate != other.DiscontinuedDate)
                return false;
            if (FinishedGoodsFlag != other.FinishedGoodsFlag)
                return false;
            if (ListPrice != other.ListPrice)
                return false;
            if (MakeFlag != other.MakeFlag)
                return false;
            if (ModifiedDate != other.ModifiedDate)
                return false;
            if (Name != other.Name)
                return false;
            if (ProductLine != other.ProductLine)
                return false;
            if (ProductModelId != other.ProductModelId)
                return false;
            if (ProductNumber != other.ProductNumber)
                return false;
            if (ProductSubcategoryId != other.ProductSubcategoryId)
                return false;
            if (ReorderPoint != other.ReorderPoint)
                return false;
            if (Rowguid != other.Rowguid)
                return false;
            if (SafetyStockLevel != other.SafetyStockLevel)
                return false;
            if (SellEndDate != other.SellEndDate)
                return false;
            if (SellStartDate != other.SellStartDate)
                return false;
            if (Size != other.Size)
                return false;
            if (SizeUnitMeasureCode != other.SizeUnitMeasureCode)
                return false;
            if (StandardCost != other.StandardCost)
                return false;
            if (Style != other.Style)
                return false;
            if (Weight != other.Weight)
                return false;
            if (WeightUnitMeasureCode != other.WeightUnitMeasureCode)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Class == null ? 0 : Class.GetHashCode());
                hash = hash * 23 + (Color == null ? 0 : Color.GetHashCode());
                hash = hash * 23 + (DaysToManufacture == default(int) ? 0 : DaysToManufacture.GetHashCode());
                hash = hash * 23 + (DiscontinuedDate == null ? 0 : DiscontinuedDate.GetHashCode());
                hash = hash * 23 + (FinishedGoodsFlag == default(bool) ? 0 : FinishedGoodsFlag.GetHashCode());
                hash = hash * 23 + (ListPrice == default(decimal) ? 0 : ListPrice.GetHashCode());
                hash = hash * 23 + (MakeFlag == default(bool) ? 0 : MakeFlag.GetHashCode());
                hash = hash * 23 + (ModifiedDate == default(DateTime) ? 0 : ModifiedDate.GetHashCode());
                hash = hash * 23 + (Name == null ? 0 : Name.GetHashCode());
                hash = hash * 23 + (ProductLine == null ? 0 : ProductLine.GetHashCode());
                hash = hash * 23 + (ProductModelId == null ? 0 : ProductModelId.GetHashCode());
                hash = hash * 23 + (ProductNumber == null ? 0 : ProductNumber.GetHashCode());
                hash = hash * 23 + (ProductSubcategoryId == null ? 0 : ProductSubcategoryId.GetHashCode());
                hash = hash * 23 + (ReorderPoint == default(short) ? 0 : ReorderPoint.GetHashCode());
                hash = hash * 23 + (Rowguid == default(Guid) ? 0 : Rowguid.GetHashCode());
                hash = hash * 23 + (SafetyStockLevel == default(short) ? 0 : SafetyStockLevel.GetHashCode());
                hash = hash * 23 + (SellEndDate == null ? 0 : SellEndDate.GetHashCode());
                hash = hash * 23 + (SellStartDate == default(DateTime) ? 0 : SellStartDate.GetHashCode());
                hash = hash * 23 + (Size == null ? 0 : Size.GetHashCode());
                hash = hash * 23 + (SizeUnitMeasureCode == null ? 0 : SizeUnitMeasureCode.GetHashCode());
                hash = hash * 23 + (StandardCost == default(decimal) ? 0 : StandardCost.GetHashCode());
                hash = hash * 23 + (Style == null ? 0 : Style.GetHashCode());
                hash = hash * 23 + (Weight == null ? 0 : Weight.GetHashCode());
                hash = hash * 23 + (WeightUnitMeasureCode == null ? 0 : WeightUnitMeasureCode.GetHashCode());
                return hash;
            }
        }
        public static bool operator ==(Product left, Product right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Product left, Product right)
        {
            return !Equals(left, right);
        }

        #endregion Equals/GetHashCode

        #region INotifyPropertyChanged/IsDirty
        public HashSet<string> ChangedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public void MarkAsClean()
        {
            ChangedProperties.Clear();
        }
        public virtual bool IsDirty => ChangedProperties.Any();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void SetField<T>(ref T field, T value, string propertyName)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                ChangedProperties.Add(propertyName);
                OnPropertyChanged(propertyName);
            }
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged/IsDirty
    }
}

