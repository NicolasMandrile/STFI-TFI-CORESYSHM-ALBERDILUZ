-- ============================================================
-- CoreSys HM - Script 01: Creación de Base de Datos
-- Sistema de Gestión Operativa (Stock, Ventas, Facturación)
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CoreSysHM')
BEGIN
    CREATE DATABASE CoreSysHM
    COLLATE Modern_Spanish_CI_AI;
END
GO

USE CoreSysHM;
GO
