   M S B A M L   `   `   `  ����   WPUCabExtractor     WPUCabExtractor.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$+  %Windows Update CAB 提取重建工具��$	��400��$	��600��$	��525��$	��350�� � Icon$ Assets/Icon/Icon.ico��.��� $��10q� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* ��� $
��Auto �.+�� $��0��.+��� $����本工具根据已解压缩的 Windows Phone Update 描述文件 (dsm 文件) 中的描述，从源目录抽取文件重构系统目录结构或包结构。��$
��Wrap=�� $��1��$��0,10,0,0q� �� $
��Auto ��� $
��Auto �.+��� $��0��$��DSM 文件路径:��� $��1��$��0,5,0,0q� � ColumnDefinitions �� $��* ��� $
��Auto �.+��� -    ��Name$ txtInputDir��$��0�� �� 
IsReadOnly .  �� IsReadOnlyCaretVisible . $��0,0,5,0q��� -    ��Name$ btnBrowseInput��$��1��$��浏览(_B)...��$��55��� $��2��$��0,5,0,0q� �� $
��Auto ��� $
��Auto �.+��� $��0��$"��用于抽取文件的路径:��� $��1��$��0,5,0,0q� �� $��* ��� $
��Auto �.+��� -   $ txtResourceDir��$��0�� .  . $��0,0,5,0q��� -   $ btnBrowseResource��$��1��$��浏览(_B)...��$��55��� $��3��$��0,5,0,0q� �� $
��Auto ��� $
��Auto �.+��� $��0��$(��"重建的目录或包结构路径:��� $��1��$��0,5,0,0q� �� $��* ��� $
��Auto �.+��� -   $ txtOutputDir��$��0�� .  . $��0,0,5,0q��� -   $ btnBrowseOutput��$��1��$��浏览(_B)...��$��55��� $��4��$��0,10,0,0q� �� $
��Auto ��� $
��Auto ��� $��* �.+��� -   $ btnStart��$��0��$��
开始(_S)��$��25��$��50���� -    ��Name$ chkKeepStructure��$��1��$O��I抽取的文件保持原始目录结构，而不要整理为包结构(_K)��$��Center=�$��10,0,0,0q��� -	   	 ��Name$	 
lstMessage��$��5��$��0,10,0,0q�� -
   
 �Name$
 prgProgress��$��6�� � Minimum$ 0Q� � Maximum$	 100Q�$��10��$��0,10,0,0q� � TaskbarItemInfo *  $System.Windows.Shell.TaskbarItemInfo  -   TaskbarItem��