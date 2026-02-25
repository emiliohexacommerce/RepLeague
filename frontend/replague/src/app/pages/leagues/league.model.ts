export interface League {
  id: string;
  name: string;
  description?: string;
  ownerId: string;
  memberCount: number;
  createdAt: string;
}

export interface LeagueMember {
  userId: string;
  displayName: string;
  avatarUrl?: string;
  joinedAt: string;
  isOwner: boolean;
}

export interface RankingEntry {
  userId: string;
  displayName: string;
  avatarUrl?: string;
  points: number;
  totalPrs: number;
  rank: number;
}

export interface CreateLeagueRequest {
  name: string;
  description?: string;
}
