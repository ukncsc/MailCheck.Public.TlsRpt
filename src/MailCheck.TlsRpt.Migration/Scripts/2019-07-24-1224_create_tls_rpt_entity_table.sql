CREATE TABLE `tls_rpt_entity` (
  `id` varchar(255) NOT NULL,
  `version` int(11) NOT NULL,
  `state` json NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;