﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Clip Library")]
public class ClipLibrary : ScriptableObject {

    [SerializeField] private AudioClip mainTrackAudio;
    public AudioClip MainTrackAudio { get { return mainTrackAudio; } }

    [SerializeField] private AudioClip piecePlaced;
    public AudioClip PiecePlaced { get { return piecePlaced; } }

    [SerializeField] private AudioClip piecePicked;
    public AudioClip PiecePicked { get { return piecePicked; } }

    [SerializeField] private AudioClip pieceDrawn;
    public AudioClip PieceDrawn { get { return pieceDrawn; } }

    [SerializeField] private AudioClip pieceDestroyed;
    public AudioClip PieceDestroyed { get { return pieceDestroyed; } }

    [SerializeField] private AudioClip blueprintPlaced;
    public AudioClip BlueprintPlaced { get { return blueprintPlaced; } }

    [SerializeField] private AudioClip prodLevelUp;
    public AudioClip ProdLevelUp { get { return prodLevelUp; } }

    [SerializeField] private AudioClip structureClaimed;
    public AudioClip StructureClaimed { get { return structureClaimed; } }

    [SerializeField] private AudioClip resourceGained;
    public AudioClip ResourceGained { get { return resourceGained; } }

    [SerializeField] private AudioClip illegalPlay;
    public AudioClip IllegalPlay { get { return illegalPlay; } }

    [SerializeField] private AudioClip shieldHit;
    public AudioClip ShieldHit { get { return shieldHit; } }

    [SerializeField] private AudioClip warning;
    public AudioClip Warning { get { return warning; } }

    [SerializeField] private AudioClip victory;
    public AudioClip Victory { get { return victory; } }

    [SerializeField] private AudioClip defeat;
    public AudioClip Defeat { get { return defeat; } }
}
