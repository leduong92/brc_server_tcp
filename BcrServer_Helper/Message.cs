using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer_Helper
{
    public static class Message
    {
        public static Dictionary<int, string> MessageDictionary = new Dictionary<int, string>()
        {
            {999,"" },
            {888, "No Data Found!!!" },
            {777, "Error!!!" },
            {666, "This user doesn't exists!!!" },
            {0, "Hoan Thanh!!!"},
            {1,  "01. De lai." },
            {2,  "02. Khong lay duoc so pallete" },
            {3,  "03. PCL nay da duoc nhan" },
            {4,  "04. Khong tim thay thong tin pallete" },
            {5,  "05. Thung nay da duoc doi sang pallete moi" },
            {6,  "06. parent_pallete_no is blank" },
            {7,  "07. Khong the cap nhat so pallete moi" },
            {8,  "08. Thung nay chua duoc sap xep"},
            {9,  "09. Thung nay da SHIP."},
            {10, "10. Khong lay duoc du lieu thung" },
            {11, "11. PCL No is blank" },
            {12, "12. Quet khong dung ma vach PCL" },
            {14, "14. Box No is blank" },
            {15, "15. Quet khong dung ma vach thung" },
            {16, "16. Khong tim thay thung o trang thai da sap xep" },
            {17, "17. Khong Insert duoc box_not_in_pallete" },
            {18, "18. Sai noi den. Khong the len container" },
            {19, "19. Ma so container khong dung." },
            {20, "20. Pallete nay khong ton tai." },
            {21, "21. Khong the update du lieu container." },
            {22, "22. Pallete nay da len cont." },
            {23, "23. Pallete nay chua duoc scan SHIP" },
            {24, "24. Container nay da duoc scan ket thuc." },
            {25, "25. Khong the cap nhat container_lock_sign." },
            {26, "26. Container nay da duoc chot." },
            {27, "27. Khong the insert tt_unposedjob_nbcs" },
            {28, "28. Khong the cap nhat du lieu td_box_delivery" },
            {29, "29. Khong co thung." },
            {30, "30. Box chua ghi nhan FI"}, //duong
            {31, "31. PCL da duoc nhan."},	 //duong
            {32, "32. User khong hop le."},
            {33, "33. Khong the insert td_check_stock."},
            {34, "34. Khong the sap xep box nay."},
            {35, "35. Thung nay da sap xep o pallete khac."},
            {36, "36. Khong the tim thay thong tin Pallete!!."},
            {37, "37. Sai noi den!!."},
            {38, "38. Khong tim thay Thung!!"},
            {39, "39. Fifo Ko tim thay!"},
            {40, "40. Khong the them vao td_box_info"},
            {41, "41. Fifo Khong the them d/l td_box_fifo"},
            {42, "42. Khong tim thay Shipping To"}, //duong
            {43, "43. Khong the them vao ts_stock_result"},
            {44, "44. Khong the update ts_stock_wh"},
            {45, "45. Khong the them vao ts_stock_wh"},
            {46, "46. Fifo"},
            {47, "47. Khong the tim thay Thung trong pallet nay"},
            {48, "48. Khong the cap nhap du lieu Box Info."},
            {49, "49. Khong co du lieu warehouse plan."},
            {50, "50. De lai."},
            {51, "51. De lai No fix."},
            {52, "52. Khong tim thay noi den"}, //duong
            {53, "53. Can't get Seq"}, //duong
            {54, "54. Khong the them du lieu Shipping print"}, //duong
            {55, "55. Khong the cap nhat Shipping print"}, //duong
            {56, "56. Khong the cap nhat Box Info"}, //duong
            {57, "57. Khong the hem du lieu TsBoxTrace"}, //duong
            {58, "58. Thung nay da duoc sap xep"},
            {59, "59. Thung nay da duoc in Shipping To"},
            {60, "60. Thung nay da duoc Reser"},
            {61, "61. Thung nay da duoc Ship"},
            {62, "62. Khong the them vao td_control_group_seq"},
            {63, "63. Khong the update td_control_group_seq "},
            {64, "64. Khong the them vao td_rec_code_wh"},
            {65, "65. Khong the update td_rec_code_wh "},
            {66, "66. Khong the update td_box_info "},
            {67, "67. Khong the them vao td_pallete_wh"},
            {68, "68. Khong the them vao td_pallete_print"},
            {69, "69. Khong lay duoc so luong tren pallete" },
            {70, "70. Khong cap nhat duoc du lieu tr_mps_info_nbcs" },
            {71, "71. Khong cap nhat duoc du lieu td_incoming_box"},
            {72, "72. Khong inset duoc du lieu td_box_return"},
            {73, "73. Khong the xoa du lieu td_box_delivery"},
            {74, "74. Khong the xoa du lieu td_box_info"},
            {75, "75. Khong the xoa du lieu tt_pcl_print"},
            {76, "76. Khong the xoa du lieu td_daily_box_rec"},
            {77, "77. Thung nay dang cho san xuat muon."},
            {78, "78. Khong co du lieu in Shipping To/Mark."},
            {79, "79. Khong the cap nhat check stock"}, //duong
            {80, "80. Khong co du lieu palletetype_no"}, //duong
            {81, "81. Khong tim thay shipping to"},
            {82, "82. Khong the update td_pallete_wh"},
            {83, "83. Shipping To Khong o trang thai Reser hoac Ship"},
            {84, "84. Shipping To Khong o trang thai Reser"},
            {85, "85. Shipping To Khong o trang thai Ship"},
            {86, "86. Thung nay da duoc sap xep len pallete."},
            {87, "87. Loi CheckFifo"},
            {88, "88. Loi InsOrUpFF - CheckFiFoForReser()" },
            {89, "89. Thung nay chua duoc nhan PCL" },
            {90, "90. Khong the xoa du lieu tt_shipping_print" },
            {91, "91. Khong co du lieu in PCL" },
            {92, "92. Khong the cap nhat packing_user" },
            {93, "93. Khong phai box hang cho" },
            {94, "94. Shipping to da du hang cho" },
            {95, "95. Khong Update duoc actual qty" },
            {96, "96. Box hang cho khong thuoc Shipping to nay" },
            {97, "97. Pallete co chua 2 noi den khac nhau" },
            {98, "98. Item Chua du hang" },
            {99, "99. Khong xoa duoc du lieu trong waiting_plan_log" },
            {100,"100. Lay thung nay ra." },
            {101, "101. Thung nay chua scan nhan hang" },
            {102, "Pallete co hang cho" },
            {103, "103. PALLETE DA DU HANG CHO" },
            {104, "104. KHONG PHAI PALLETE HANG CHO" },
            {105, "105. Khong cap nhat duoc shipping cho box" },
            {106, "106. HANG CHO CON THIEU" },
            {107, "107. Thung nay da duoc them vao shipping to" }
        };
    }
}
