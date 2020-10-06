CREATE TABLE `tls_rpt_scheduled_records` (
  `id` varchar(255) NOT NULL,
  `last_checked` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_last_checked` (`last_checked`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;